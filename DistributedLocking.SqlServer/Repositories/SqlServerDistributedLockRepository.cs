using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using TheName.DistributedLocking.Abstractions.Records;
using TheName.DistributedLocking.Abstractions.Repositories;
using TheName.DistributedLocking.SqlServer.Abstractions.Configuration;
using TheName.DistributedLocking.SqlServer.Helpers;

namespace TheName.DistributedLocking.SqlServer.Repositories
{
    public class SqlServerDistributedLockRepository : IDistributedLockRepository
    {
        private const string LockIdentifierParameterName = "@LockIdentifier";
        private const string LockIdParameterName = "@LockId";
        private const string ExpiryDateTimeSpanInMillisecondsParameterName = "@ExpiryDateTimeSpanInMilliseconds";

        private static readonly string InsertDistributedLockIfNotExistsSqlCommand =
            $"INSERT INTO [{SqlServerDistributedLocksTableHelper.SchemaName}].[{SqlServerDistributedLocksTableHelper.TableName}] " +
            "SELECT " +
            $"   {LockIdentifierParameterName}," +
            $"   {LockIdParameterName}," +
            $"   DATEADD(millisecond,{ExpiryDateTimeSpanInMillisecondsParameterName},SYSUTCDATETIME()) " +
            "WHERE" +
            "   NOT EXISTS " +
            "   (SELECT *" +
            $"   FROM [{SqlServerDistributedLocksTableHelper.SchemaName}].[{SqlServerDistributedLocksTableHelper.TableName}] WITH (UPDLOCK, HOLDLOCK)" +
            $"   WHERE  LockIdentifier = {LockIdentifierParameterName}" +
            "    AND    ExpiryDateTimestamp < SYSUTCDATETIME());";

        private static readonly string DeleteDistributedLockIfExistsSqlCommand =
            $"DELETE FROM [{SqlServerDistributedLocksTableHelper.SchemaName}].[{SqlServerDistributedLocksTableHelper.TableName}] " +
            $"WHERE LockId = {LockIdParameterName} " +
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
            await SqlServerDistributedLocksTableHelper.CreateTableIfNotExistsAsync(_configuration.ConnectionString, cancellationToken).ConfigureAwait(false);
            int numberOfAffectedRows;
            var lockId = Guid.NewGuid();
            var connection = new SqlConnection(_configuration.ConnectionString);
            await using (connection.ConfigureAwait(false))
            {
                var command = new SqlCommand(InsertDistributedLockIfNotExistsSqlCommand, connection);
                command.Parameters.Add(LockIdentifierParameterName, SqlDbType.Char, 36).Value = lockIdentifier.Value.ToString();
                command.Parameters.Add(LockIdParameterName, SqlDbType.Char, 36).Value = lockId.ToString();
                command.Parameters.Add(ExpiryDateTimeSpanInMillisecondsParameterName, SqlDbType.Int).Value = lockTimeout.Value.TotalMilliseconds;
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
            await SqlServerDistributedLocksTableHelper.CreateTableIfNotExistsAsync(_configuration.ConnectionString, cancellationToken).ConfigureAwait(false);
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
    }
}