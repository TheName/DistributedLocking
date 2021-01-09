using System;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedLocking.SqlServer.Abstractions.Helpers
{
    internal interface ISqlDataDefinitionLanguageExecutor
    {
        Task<bool> TableExistsAsync(
            string schemaName,
            string tableName,
            CancellationToken cancellationToken);

        Task CreateTableIfNotExistsAsync(
            string schemaName,
            string tableName,
            TimeSpan applicationLockTimeout,
            string createTableSqlCommandText,
            CancellationToken cancellationToken);
    }
}