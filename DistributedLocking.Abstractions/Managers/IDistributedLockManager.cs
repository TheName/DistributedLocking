using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.Abstractions.Records;

namespace DistributedLocking.Abstractions.Managers
{
    public interface IDistributedLockManager
    {
        Task<IDistributedLock> AcquireAsync(
            DistributedLockIdentifier lockIdentifier,
            DistributedLockTimeToLive lockTimeToLive,
            DistributedLockAcquiringTimeout acquiringTimeout,
            DistributedLockAcquiringDelayBetweenRetries delayBetweenRetries,
            CancellationToken cancellationToken);

        Task ExtendAsync(
            IDistributedLock distributedLock,
            DistributedLockTimeToLive lockTimeToLive,
            CancellationToken cancellationToken);

        Task ReleaseAsync(IDistributedLock distributedLock, CancellationToken cancellationToken);
    }
}