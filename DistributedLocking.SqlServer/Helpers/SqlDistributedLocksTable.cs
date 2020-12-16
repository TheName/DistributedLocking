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
        
        private const string CreateDistributedLockTableSqlCommandFormat =
            "CREATE TABLE [{0}].{1} ( " +
            "   LockIdentifier          CHAR(36)    NOT NULL UNIQUE, " +
            "   LockId                  CHAR(36)    NOT NULL UNIQUE, " +
            "   ExpiryDateTimestamp     DATETIME2   NOT NULL, " +
            "   CONSTRAINT PK_{1}       PRIMARY KEY (LockIdentifier)); " +
            "CREATE INDEX IDX_LockIdentifier_ExpiryDateTimestamp ON [{0}].{1} (LockIdentifier, ExpiryDateTimestamp); " +
            "CREATE INDEX IDX_LockId_ExpiryDateTimestamp ON [{0}].{1} (LockId, ExpiryDateTimestamp); ";

        private static readonly string InsertIfNotExistsSqlCommandFormat =
            "BEGIN TRANSACTION; " +
            "SET NOCOUNT ON " +
            "DELETE FROM [{0}].[{1}] " +
            $"  WHERE LockIdentifier = {LockIdentifierParameterName} AND ExpiryDateTimestamp < SYSUTCDATETIME(); " +
            "SET NOCOUNT OFF " +
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
            "AND    ExpiryDateTimestamp < SYSUTCDATETIME();";
        
        private readonly ISqlClient _sqlClient;
        private readonly ISqlDataDefinitionLanguageExecutor _dataDefinitionLanguageExecutor;

        public SqlDistributedLocksTable(
            ISqlClient sqlClient,
            ISqlDataDefinitionLanguageExecutor dataDefinitionLanguageExecutor)
        {
            _sqlClient = sqlClient ?? throw new ArgumentNullException(nameof(sqlClient));
            _dataDefinitionLanguageExecutor = dataDefinitionLanguageExecutor ?? throw new ArgumentNullException(nameof(dataDefinitionLanguageExecutor));
        }

        public async Task<bool> TableExistsAsync(string schemaName, string tableName, CancellationToken cancellationToken) =>
            await _dataDefinitionLanguageExecutor.TableExistsAsync(schemaName, tableName, cancellationToken)
                .ConfigureAwait(false);

        public async Task CreateTableIfNotExistsAsync(
            string schemaName,
            string tableName,
            TimeSpan sqlApplicationLockTimeout,
            CancellationToken cancellationToken) =>
            await _dataDefinitionLanguageExecutor.CreateTableIfNotExistsAsync(
                    schemaName,
                    tableName,
                    sqlApplicationLockTimeout,
                    GetCreateTableSqlCommand(schemaName, tableName),
                    cancellationToken)
                .ConfigureAwait(false);

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
        
        private static string GetCreateTableSqlCommand(string schemaName, string tableName) =>
            string.Format(
                CreateDistributedLockTableSqlCommandFormat,
                schemaName,
                tableName);

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