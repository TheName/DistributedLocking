using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using TheName.DistributedLocking.Abstractions.Records;
using TheName.DistributedLocking.Abstractions.Repositories;
using TheName.DistributedLocking.SqlServer.Abstractions.Configuration;

namespace TheName.DistributedLocking.SqlServer.Repositories
{
    public class SqlServerDistributedLockRepository : IDistributedLockRepository
    {
        private const string SqlSchemaName = "TheName_DistributedLocking_Sql";
        private const string SqlTableName = "DistributedLocks";
        private const string LockIdentifierParameterName = "@LockIdentifier";
        private const string LockIdParameterName = "@LockId";
        private const string ExpiryDateTimeSpanInMillisecondsParameterName = "@ExpiryDateTimeSpanInMilliseconds";
        private const int CreateSchemaAndTableIfNotExistsLockTimeoutInMilliseconds = 3000;

        private static bool _wasSchemaAndTableCreated = false;
        private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);
        private static readonly string CreateSchemaAndTableIfNotExistsSqlCommand = 
            "BEGIN TRANSACTION; " +
            "DECLARE @getapplock_result int; " +
            $"EXEC @getapplock_result = sp_getapplock @Resource = '{SqlSchemaName}', @LockMode = 'Exclusive', @LockTimeout = {CreateSchemaAndTableIfNotExistsLockTimeoutInMilliseconds}; " +
            "IF @getapplock_result < 0 " +
            "BEGIN" +
            "   ROLLBACK TRANSACTION;" +
            "   SELECT @getapplock_result;" +
            "END" +
            "ELSE" +
            "BEGIN " +
            $"  IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{SqlSchemaName}')) " +
            "   BEGIN " +
            $"      CREATE SCHEMA [{SqlSchemaName}] " +
            "   END " +
            "   BEGIN" +
            $"      CREATE TABLE [{SqlSchemaName}].{SqlTableName} ( " +
            "           LockId                  CHAR(36)    NOT NULL UNIQUE, " +
            "           LockIdentifier          CHAR(36)    NOT NULL UNIQUE, " +
            "           ExpiryDateTimestamp     DATETIME2   NOT NULL, " +
            $"          CONSTRAINT PK_{SqlTableName} PRIMARY KEY (LockId)); " +
            $"      CREATE INDEX IDX_LockIdentifier_ExpiryDateTimestamp ON [{SqlSchemaName}].{SqlTableName} (LockIdentifier, ExpiryDateTimestamp); " +
            $"      CREATE INDEX IDX_LockId_ExpiryDateTimestamp ON [{SqlSchemaName}].{SqlTableName} (LockId, ExpiryDateTimestamp); " +
            "       COMMIT TRANSACTION;  " +
            "       SELECT @getapplock_result;" +
            "   END " +
            "END; ";

        private static readonly string InsertDistributedLockIfNotExistsSqlCommand =
            $"INSERT INTO [{SqlSchemaName}].[{SqlTableName}] " +
            "SELECT " +
            $"   '{LockIdentifierParameterName}'," +
            $"   '{LockIdParameterName}'," +
            $"   DATEADD(millisecond,{ExpiryDateTimeSpanInMillisecondsParameterName},SYSUTCDATETIME()) " +
            "WHERE" +
            "   NOT EXISTS " +
            "   (SELECT *" +
            $"   FROM [{SqlSchemaName}].[{SqlTableName}] WITH (UPDLOCK, HOLDLOCK)" +
            $"   WHERE  LockIdentifier = '{LockIdentifierParameterName}'" +
            "    AND    ExpiryDateTimestamp < SYSUTCDATETIME());";

        private static readonly string DeleteDistributedLockIfExistsSqlCommand =
            $"DELETE FROM [{SqlSchemaName}].[{SqlTableName}] " +
            $"WHERE LockId = '{LockIdParameterName}' " +
            "AND    ExpiryDateTimestamp < SYSUTCDATETIME();";
            
        
        private readonly ISqlServerDistributedLockConfiguration _configuration;

        public SqlServerDistributedLockRepository(ISqlServerDistributedLockConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }
        
        public async Task<(bool Success, LockId AcquiredLockId)> TryAcquireAsync(
            LockIdentifier lockIdentifier,
            LockTimeout lockTimeout,
            CancellationToken cancellationToken)
        {
            await CreateTableIfDoesNotExistIfWasNotExecutedAsync(cancellationToken).ConfigureAwait(false);
            int numberOfAffectedRows;
            var lockId = Guid.NewGuid();
            var connection = new SqlConnection(_configuration.ConnectionString);
            await using (connection.ConfigureAwait(false))
            {
                var command = new SqlCommand(InsertDistributedLockIfNotExistsSqlCommand, connection);
                command.Parameters.Add(LockIdentifierParameterName, SqlDbType.Char, 36).Value = lockIdentifier.Value;
                command.Parameters.Add(LockIdParameterName, SqlDbType.Char, 36).Value = lockId;
                command.Parameters.Add(ExpiryDateTimeSpanInMillisecondsParameterName, SqlDbType.Int, (int) lockTimeout.Value.TotalMilliseconds);
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                numberOfAffectedRows = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            return numberOfAffectedRows switch
            {
                0 => (false, null),
                1 => (true, new LockId(lockId)),
                _ => throw new InvalidOperationException(
                    $"Insert distributed lock statement should have affected 1 or 0 rows, but it affected {numberOfAffectedRows}")
            };
        }

        public async Task<bool> TryReleaseAsync(LockId lockId, CancellationToken cancellationToken)
        {
            await CreateTableIfDoesNotExistIfWasNotExecutedAsync(cancellationToken).ConfigureAwait(false);
            int numberOfAffectedRows;
            var connection = new SqlConnection(_configuration.ConnectionString);
            await using (connection.ConfigureAwait(false))
            {
                var command = new SqlCommand(DeleteDistributedLockIfExistsSqlCommand, connection);
                command.Parameters.Add(LockIdParameterName, SqlDbType.Char, 36).Value = lockId;
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                numberOfAffectedRows = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            return numberOfAffectedRows switch
            {
                0 => false,
                1 => true,
                _ => throw new InvalidOperationException(
                    $"Delete distributed lock statement should have affected 1 or 0 rows, but it affected {numberOfAffectedRows}")
            };
        }

        private async Task CreateTableIfDoesNotExistIfWasNotExecutedAsync(CancellationToken cancellationToken)
        {
            if (_wasSchemaAndTableCreated)
            {
                return;
            }

            await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            if (_wasSchemaAndTableCreated)
            {
                Semaphore.Release();
            }

            await CreateTableIfDoesNotExistAsync(cancellationToken).ConfigureAwait(false); 
            _wasSchemaAndTableCreated = true;
            Semaphore.Release();
        }

        private async Task CreateTableIfDoesNotExistAsync(CancellationToken cancellationToken)
        {
            var connection = new SqlConnection(_configuration.ConnectionString);
            object result;
            await using (connection.ConfigureAwait(false))
            {
                var command = new SqlCommand(CreateSchemaAndTableIfNotExistsSqlCommand, connection);
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            }

            if (result is not int resultInt)
            {
                throw new InvalidOperationException("SQL script creating schema and distributed lock table should have returned a result status code.");
            }

            switch ((GetAppLockResult)resultInt)
            {
                case GetAppLockResult.Successful:
                case GetAppLockResult.SuccessfulAfterWaiting:
                    return;
                
                case GetAppLockResult.Timeout:
                    throw new TimeoutException(
                        $"SQL script creating schema and distributed lock table did not succeed because getting SQL application lock timeout out (timeout set to {CreateSchemaAndTableIfNotExistsLockTimeoutInMilliseconds} ms). Please try again");
                
                case GetAppLockResult.Cancelled:
                case GetAppLockResult.Deadlock:
                case GetAppLockResult.ParameterValidationOrOtherError:
                    throw new InvalidOperationException(
                        "SQL script creating schema and distributed lock table failed to acquire application lock.");
                
                default:
                    throw new InvalidOperationException(
                        "Unexpected error during SQL script creating schema and distributed lock table execution");
            }
        }
        
        private enum GetAppLockResult
        {
            Successful = 0,
            SuccessfulAfterWaiting = 1,
            Timeout = -1,
            Cancelled = -2,
            Deadlock = -3,
            ParameterValidationOrOtherError = -999
        }
    }
}