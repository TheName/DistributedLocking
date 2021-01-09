using System;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedLocking.SqlServer.Abstractions.Helpers
{
    internal interface ISqlDistributedLocksTableManager
    {
        Task<bool> TableExistsAsync(
            string schemaName,
            string tableName,
            CancellationToken cancellationToken);
        
        Task CreateTableIfNotExistsAsync(
            string schemaName,
            string tableName,
            TimeSpan sqlApplicationLockTimeout,
            CancellationToken cancellationToken);
    }
}