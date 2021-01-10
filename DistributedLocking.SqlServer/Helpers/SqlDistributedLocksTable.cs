using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.SqlServer.Abstractions.Helpers;

namespace DistributedLocking.SqlServer.Helpers
{
    internal class SqlDistributedLocksTable : ISqlDistributedLocksTable
    {
        private const string IdentifierParameterName = "@Identifier";
        private const string IdParameterName = "@Id";
        private const string ExpiryDateTimeSpanInMillisecondsParameterName = "@ExpiryDateTimeSpanInMilliseconds";

        private static readonly string InsertIfNotExistsSqlCommandFormat =
            "BEGIN TRANSACTION; " +
            "   SET NOCOUNT ON; " +
            "   DELETE" + 
            "       FROM [{0}].[{1}] " +
            $"      WHERE Identifier = {IdentifierParameterName} AND ExpiryDateTimestamp < SYSUTCDATETIME(); " +
            "   SET NOCOUNT OFF; " +
            "COMMIT TRANSACTION; " +
            "BEGIN TRANSACTION; " +
            "   INSERT INTO [{0}].[{1}] " +
            "       SELECT " +
            $"          {IdentifierParameterName}," +
            $"          {IdParameterName}," +
            $"          DATEADD(millisecond,{ExpiryDateTimeSpanInMillisecondsParameterName},SYSUTCDATETIME()) " +
            "       WHERE" +
            "           NOT EXISTS " +
            "           (SELECT *" +
            "           FROM [{0}].[{1}] WITH (UPDLOCK, HOLDLOCK)" +
            $"          WHERE  Identifier = {IdentifierParameterName}" +
            "           AND    ExpiryDateTimestamp > SYSUTCDATETIME()); " +
            "COMMIT TRANSACTION; ";

        private static readonly string DeleteDistributedLockIfExistsSqlCommandFormat =
            "DELETE " +
            "   FROM [{0}].[{1}] " +
            $"  WHERE   Identifier = {IdentifierParameterName}" +
            $"  AND     Id = {IdParameterName} " +
            "   AND     ExpiryDateTimestamp > SYSUTCDATETIME();";

        private static readonly string UpdateDistributedLockIfExistsSqlCommandFormat =
            "BEGIN TRANSACTION; " +
            "   UPDATE [{0}].[{1}] " +
            "   SET" +
            $"      ExpiryDateTimestamp = DATEADD(millisecond,{ExpiryDateTimeSpanInMillisecondsParameterName},SYSUTCDATETIME()) " +
            $"  WHERE   Identifier = {IdentifierParameterName}" +
            $"  AND     Id = {IdParameterName} " +
            "   AND     EXISTS " +
            "       (SELECT *" +
            "       FROM [{0}].[{1}] WITH (UPDLOCK, HOLDLOCK)" +
            $"      WHERE  Id = {IdParameterName} " +
            "       AND    ExpiryDateTimestamp > SYSUTCDATETIME()); " +
            "COMMIT TRANSACTION; ";
        
        private readonly ISqlClient _sqlClient;

        public SqlDistributedLocksTable(ISqlClient sqlClient)
        {
            _sqlClient = sqlClient ?? throw new ArgumentNullException(nameof(sqlClient));
        }

        public async Task<bool> TryInsertAsync(
            string schemaName,
            string tableName,
            Guid identifier,
            Guid id,
            TimeSpan timeToLiveTimeSpan,
            CancellationToken cancellationToken)
        {
            var numberOfAffectedRows = await _sqlClient.ExecuteNonQueryAsync(
                    GetInsertDistributedLockIfNotExistsSqlCommandText(schemaName, tableName),
                    new[]
                    {
                        GetIdentifierParameter(identifier),
                        GetIdParameter(id),
                        GetExpiryDateTimeSpanInMillisecondsParameter(timeToLiveTimeSpan)
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

        public async Task<bool> TryUpdateAsync(
            string schemaName,
            string tableName,
            Guid identifier,
            Guid id,
            TimeSpan additionalTimeToLiveTimeSpan,
            CancellationToken cancellationToken)
        {
            var numberOfAffectedRows = await _sqlClient.ExecuteNonQueryAsync(
                    GetUpdateDistributedLockIfExistsSqlCommandText(schemaName, tableName),
                    new[] 
                    {
                        GetIdentifierParameter(identifier),
                        GetIdParameter(id),
                        GetExpiryDateTimeSpanInMillisecondsParameter(additionalTimeToLiveTimeSpan)
                    },
                    cancellationToken)
                .ConfigureAwait(false);

            return numberOfAffectedRows switch
            {
                0 => false,
                1 => true,
                _ => throw new InvalidOperationException(
                    $"Update distributed lock statement should have affected 1 or 0 rows, but it affected {numberOfAffectedRows}")
            };
        }

        public async Task<bool> TryDeleteAsync(
            string schemaName,
            string tableName,
            Guid identifier,
            Guid id,
            CancellationToken cancellationToken)
        {
            var numberOfAffectedRows = await _sqlClient.ExecuteNonQueryAsync(
                    GetDeleteDistributedLockIfExistsSqlCommandText(schemaName, tableName),
                    new[]
                    {
                        GetIdentifierParameter(identifier),
                        GetIdParameter(id)
                    },
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

        private static string GetUpdateDistributedLockIfExistsSqlCommandText(string schemaName, string tableName) =>
            string.Format(UpdateDistributedLockIfExistsSqlCommandFormat, schemaName, tableName);

        private static SqlParameter GetIdentifierParameter(Guid identifier) =>
            new(IdentifierParameterName, SqlDbType.Char, 36)
            {
                Value = identifier.ToString()
            };

        private static SqlParameter GetIdParameter(Guid id) =>
            new(IdParameterName, SqlDbType.Char, 36)
            {
                Value = id.ToString()
            };

        private static SqlParameter GetExpiryDateTimeSpanInMillisecondsParameter(TimeSpan timeSpan) =>
            new SqlParameter(ExpiryDateTimeSpanInMillisecondsParameterName, SqlDbType.BigInt)
            {
                Value = timeSpan.TotalMilliseconds
            };
    }
}