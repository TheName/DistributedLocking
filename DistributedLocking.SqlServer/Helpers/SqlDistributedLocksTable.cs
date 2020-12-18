using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using TheName.DistributedLocking.SqlServer.Abstractions.Helpers;

namespace TheName.DistributedLocking.SqlServer.Helpers
{
    internal class SqlDistributedLocksTable : ISqlDistributedLocksTable
    {
        private const string LockIdentifierParameterName = "@LockIdentifier";
        private const string LockIdParameterName = "@LockId";
        private const string ExpiryDateTimeSpanInMillisecondsParameterName = "@ExpiryDateTimeSpanInMilliseconds";

        private static readonly string InsertIfNotExistsSqlCommandFormat =
            "BEGIN TRANSACTION; " +
            "SET NOCOUNT ON; " +
            "DELETE FROM [{0}].[{1}] " +
            $"  WHERE LockIdentifier = {LockIdentifierParameterName} AND ExpiryDateTimestamp < SYSUTCDATETIME(); " +
            "SET NOCOUNT OFF; " +
            "COMMIT TRANSACTION; " +
            "BEGIN TRANSACTION; " +
            "INSERT INTO [{0}].[{1}] " +
            "SELECT " +
            $"   {LockIdentifierParameterName}," +
            $"   {LockIdParameterName}," +
            $"   DATEADD(millisecond,{ExpiryDateTimeSpanInMillisecondsParameterName},SYSUTCDATETIME()) " +
            "WHERE" +
            "   NOT EXISTS " +
            "   (SELECT *" +
            "   FROM [{0}].[{1}] WITH (UPDLOCK, HOLDLOCK)" +
            $"   WHERE  LockIdentifier = {LockIdentifierParameterName}" +
            "    AND    ExpiryDateTimestamp > SYSUTCDATETIME()); " +
            "COMMIT TRANSACTION; ";

        private static readonly string DeleteDistributedLockIfExistsSqlCommandFormat =
            "DELETE FROM [{0}].[{1}] " +
            $"WHERE LockId = {LockIdParameterName} " +
            "AND    ExpiryDateTimestamp > SYSUTCDATETIME();";
        
        private readonly ISqlClient _sqlClient;

        public SqlDistributedLocksTable(ISqlClient sqlClient)
        {
            _sqlClient = sqlClient ?? throw new ArgumentNullException(nameof(sqlClient));
        }

        public async Task<bool> TryInsertAsync(
            string schemaName,
            string tableName,
            Guid lockIdentifier,
            Guid lockId,
            TimeSpan expirationTimeSpan,
            CancellationToken cancellationToken)
        {
            var numberOfAffectedRows = await _sqlClient.ExecuteNonQueryAsync(
                    GetInsertDistributedLockIfNotExistsSqlCommandText(schemaName, tableName),
                    new[]
                    {
                        GetLockIdentifierParameter(lockIdentifier),
                        GetLockIdParameter(lockId),
                        GetExpiryDateTimeSpanInMillisecondsParameter(expirationTimeSpan)
                    },
                    cancellationToken)
                .ConfigureAwait(false);

            return numberOfAffectedRows switch
            {
                0 => false,
                1 => true,
                _ => throw new InvalidOperationException(
                    $"Insert distributed lock statement should have affected 1 or 0 rows, but it affected {numberOfAffectedRows}")
            };
        }

        public async Task<bool> TryDeleteAsync(string schemaName, string tableName, Guid lockId, CancellationToken cancellationToken)
        {
            var numberOfAffectedRows = await _sqlClient.ExecuteNonQueryAsync(
                    GetDeleteDistributedLockIfExistsSqlCommandText(schemaName, tableName),
                    new[] {GetLockIdParameter(lockId)},
                    cancellationToken)
                .ConfigureAwait(false);

            return numberOfAffectedRows switch
            {
                0 => false,
                1 => true,
                _ => throw new InvalidOperationException(
                    $"Delete distributed lock statement should have affected 1 or 0 rows, but it affected {numberOfAffectedRows}")
            };
        }

        private static string GetInsertDistributedLockIfNotExistsSqlCommandText(string schemaName, string tableName) =>
            string.Format(InsertIfNotExistsSqlCommandFormat, schemaName, tableName);

        private static string GetDeleteDistributedLockIfExistsSqlCommandText(string schemaName, string tableName) =>
            string.Format(DeleteDistributedLockIfExistsSqlCommandFormat, schemaName, tableName);

        private static SqlParameter GetLockIdentifierParameter(Guid lockIdentifier) =>
            new(LockIdentifierParameterName, SqlDbType.Char, 36)
            {
                Value = lockIdentifier.ToString()
            };

        private static SqlParameter GetLockIdParameter(Guid lockId) =>
            new(LockIdParameterName, SqlDbType.Char, 36)
            {
                Value = lockId.ToString()
            };

        private static SqlParameter GetExpiryDateTimeSpanInMillisecondsParameter(TimeSpan timeSpan) =>
            new SqlParameter(ExpiryDateTimeSpanInMillisecondsParameterName, SqlDbType.BigInt)
            {
                Value = timeSpan.TotalMilliseconds
            };
    }
}