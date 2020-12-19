using System;
using System.Threading;
using System.Threading.Tasks;

namespace TheName.DistributedLocking.SqlServer.Abstractions.Helpers
{
    internal interface ISqlDistributedLocksTable
    {
        Task<bool> TryInsertAsync(
            string schemaName,
            string tableName,
            Guid lockIdentifier,
            Guid lockId,
            TimeSpan timeToLiveTimeSpan,
            CancellationToken cancellationToken);

        Task<bool> TryUpdateAsync(
            string schemaName,
            string tableName,
            Guid lockId,
            TimeSpan additionalTimeToLiveTimeSpan,
            CancellationToken cancellationToken);

        Task<bool> TryDeleteAsync(
            string schemaName,
            string tableName,
            Guid lockId,
            CancellationToken cancellationToken);
    }
}