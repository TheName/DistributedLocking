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
            "   Identifier              CHAR(36)    NOT NULL UNIQUE, " +
            "   Id                      CHAR(36)    NOT NULL UNIQUE, " +
            "   ExpiryDateTimestamp     DATETIME2   NOT NULL, " +
            "   CONSTRAINT PK_{1}       PRIMARY KEY (Identifier)); " +
            "CREATE INDEX IDX_Identifier_ExpiryDateTimestamp ON [{0}].{1} (Identifier, ExpiryDateTimestamp); " +
            "CREATE INDEX IDX_Id_ExpiryDateTimestamp ON [{0}].{1} (Id, ExpiryDateTimestamp); ";
        
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