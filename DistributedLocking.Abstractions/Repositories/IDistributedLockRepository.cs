using System.Threading;
using System.Threading.Tasks;
using TheName.DistributedLocking.Abstractions.Records;

namespace TheName.DistributedLocking.Abstractions.Repositories
{
    public interface IDistributedLockRepository
    {
        Task<(bool Success, DistributedLockId AcquiredLockId)> TryAcquireAsync(
            DistributedLockIdentifier lockIdentifier,
            DistributedLockTimeToLive lockTimeToLive,
            CancellationToken cancellationToken);

        Task<bool> TryExtendAsync(
            DistributedLockId lockId,
            DistributedLockTimeToLive additionalTimeToLive,
            CancellationToken cancellationToken);

        Task<bool> TryReleaseAsync(DistributedLockId lockId, CancellationToken cancellationToken);
    }
}