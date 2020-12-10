using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace TheName.DistributedLocking.SqlServer.Helpers
{
    internal static class SqlServerDistributedLocksTableHelper
    {
        internal const string SchemaName = "TheName_DistributedLocking_Sql";
        internal const string TableName = "DistributedLocks";
        private const int CreateSchemaAndTableIfNotExistsLockTimeoutInMilliseconds = 3000;

        private static bool _wasCreateTableIfNotExistsAlreadyExecuted = false;
        private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);

        private static readonly string CreateSchemaAndTableIfNotExistsSqlCommand = 
            "BEGIN TRANSACTION; " +
            "DECLARE @getapplock_result int; " +
            $"EXEC @getapplock_result = sp_getapplock @Resource = '{SchemaName}', @LockMode = 'Exclusive', @LockTimeout = {CreateSchemaAndTableIfNotExistsLockTimeoutInMilliseconds}; " +
            "IF @getapplock_result < 0 " +
            "BEGIN" +
            "   ROLLBACK TRANSACTION;" +
            "   SELECT @getapplock_result;" +
            "END " +
            "ELSE " +
            "BEGIN " +
            $"  IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{SchemaName}')) " +
            "   BEGIN " +
            $"      EXEC ('CREATE SCHEMA [{SchemaName}];') " +
            "   END " +
            "   BEGIN" +
            $"      CREATE TABLE [{SchemaName}].{TableName} ( " +
            "           LockId                  CHAR(36)    NOT NULL UNIQUE, " +
            "           LockIdentifier          CHAR(36)    NOT NULL UNIQUE, " +
            "           ExpiryDateTimestamp     DATETIME2   NOT NULL, " +
            $"          CONSTRAINT PK_{TableName} PRIMARY KEY (LockId)); " +
            $"      CREATE INDEX IDX_LockIdentifier_ExpiryDateTimestamp ON [{SchemaName}].{TableName} (LockIdentifier, ExpiryDateTimestamp); " +
            $"      CREATE INDEX IDX_LockId_ExpiryDateTimestamp ON [{SchemaName}].{TableName} (LockId, ExpiryDateTimestamp); " +
            "       COMMIT TRANSACTION;  " +
            "       SELECT @getapplock_result;" +
            "   END " +
            "END; ";

        public static async Task CreateTableIfNotExistsAsync(string connectionString, CancellationToken cancellationToken)
        {
            if (_wasCreateTableIfNotExistsAlreadyExecuted)
            {
                return;
            }

            await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            if (_wasCreateTableIfNotExistsAlreadyExecuted)
            {
                Semaphore.Release();
            }

            await CreateTableIfNotExistsInternalAsync(connectionString, cancellationToken).ConfigureAwait(false); 
            _wasCreateTableIfNotExistsAlreadyExecuted = true;
            Semaphore.Release();
        }
        
        private static async Task CreateTableIfNotExistsInternalAsync(string connectionString, CancellationToken cancellationToken)
        {
            var connection = new SqlConnection(connectionString);
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