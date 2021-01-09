using System;
using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.SqlServer.Abstractions.Helpers;

namespace DistributedLocking.SqlServer.Helpers
{
    internal class SqlDistributedLocksTableManager : ISqlDistributedLocksTableManager
    {
        private const string CreateDistributedLockTableSqlCommandFormat =
            "CREATE TABLE [{0}].{1} ( " +
            "   LockIdentifier          CHAR(36)    NOT NULL UNIQUE, " +
            "   LockId                  CHAR(36)    NOT NULL UNIQUE, " +
            "   ExpiryDateTimestamp     DATETIME2   NOT NULL, " +
            "   CONSTRAINT PK_{1}       PRIMARY KEY (LockIdentifier)); " +
            "CREATE INDEX IDX_LockIdentifier_ExpiryDateTimestamp ON [{0}].{1} (LockIdentifier, ExpiryDateTimestamp); " +
            "CREATE INDEX IDX_LockId_ExpiryDateTimestamp ON [{0}].{1} (LockId, ExpiryDateTimestamp); ";
        
        private readonly ISqlDataDefinitionLanguageExecutor _dataDefinitionLanguageExecutor;

        public SqlDistributedLocksTableManager(ISqlDataDefinitionLanguageExecutor dataDefinitionLanguageExecutor)
        {
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
        
        private static string GetCreateTableSqlCommand(string schemaName, string tableName) =>
            string.Format(
                CreateDistributedLockTableSqlCommandFormat,
                schemaName,
                tableName);
    }
}