using System;
using System.Threading;
using System.Threading.Tasks;

namespace TheName.DistributedLocking.SqlServer.Abstractions.Helpers
{
    internal interface ISqlDistributedLocksTable
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

        Task<bool> TryInsertAsync(
            string schemaName,
            string tableName,
            Guid lockIdentifier,
            Guid lockId,
            TimeSpan expirationTimeSpan,
            CancellationToken cancellationToken);

        Task<bool> TryDeleteAsync(
            string schemaName,
            string tableName,
            Guid lockId,
            CancellationToken cancellationToken);
    }
}