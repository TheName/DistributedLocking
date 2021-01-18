using System.Threading;
using System.Threading.Tasks;

namespace DistributedLocking.Abstractions.Repositories
{
    public interface IDistributedLockRepository
    {
        Task<bool> TryInsertIfIdentifierNotExistsAsync(
            DistributedLockIdentifier identifier,
            DistributedLockId id,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken);

        Task<bool> TryUpdateTimeToLiveAsync(
            DistributedLockIdentifier identifier,
            DistributedLockId id,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken);

        Task<bool> TryDeleteIfExistsAsync(
            DistributedLockIdentifier identifier,
            DistributedLockId id,
            CancellationToken cancellationToken);
    }
}