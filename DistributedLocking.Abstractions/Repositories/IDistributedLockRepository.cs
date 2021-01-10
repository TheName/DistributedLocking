using System.Threading;
using System.Threading.Tasks;

namespace DistributedLocking.Abstractions.Repositories
{
    public interface IDistributedLockRepository
    {
        Task<(bool Success, DistributedLockId AcquiredLockId)> TryAcquireAsync(
            DistributedLockIdentifier identifier,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken);

        Task<bool> TryExtendAsync(
            DistributedLockIdentifier identifier,
            DistributedLockId id,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken);

        Task<bool> TryReleaseAsync(
            DistributedLockIdentifier identifier,
            DistributedLockId id,
            CancellationToken cancellationToken);
    }
}