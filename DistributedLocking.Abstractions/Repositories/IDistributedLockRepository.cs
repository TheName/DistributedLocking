using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.Abstractions.Records;

namespace DistributedLocking.Abstractions.Repositories
{
    public interface IDistributedLockRepository
    {
        Task<(bool Success, DistributedLockId AcquiredLockId)> TryAcquireAsync(
            DistributedLockIdentifier lockIdentifier,
            DistributedLockTimeToLive lockTimeToLive,
            CancellationToken cancellationToken);

        Task<bool> TryExtendAsync(
            DistributedLockIdentifier lockIdentifier,
            DistributedLockId lockId,
            DistributedLockTimeToLive lockTimeToLive,
            CancellationToken cancellationToken);

        Task<bool> TryReleaseAsync(
            DistributedLockIdentifier lockIdentifier,
            DistributedLockId lockId,
            CancellationToken cancellationToken);
    }
}