using System.Threading;
using System.Threading.Tasks;
using TheName.DistributedLocking.Abstractions.Records;

namespace TheName.DistributedLocking.Abstractions.Managers
{
    public interface IDistributedLockManager
    {
        Task<IDistributedLock> AcquireAsync(
            DistributedLockIdentifier lockIdentifier,
            DistributedLockTimeToLive lockTimeToLive,
            DistributedLockAcquiringTimeout acquiringTimeout,
            DistributedLockAcquiringDelayBetweenRetries delayBetweenRetries,
            CancellationToken cancellationToken);

        Task ReleaseAsync(IDistributedLock @lock, CancellationToken cancellationToken);
    }
}