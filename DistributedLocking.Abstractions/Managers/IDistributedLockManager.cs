using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.Abstractions.Records;
using DistributedLocking.Abstractions.Retries;

namespace DistributedLocking.Abstractions.Managers
{
    public interface IDistributedLockManager
    {
        Task<IDistributedLock> AcquireAsync(
            DistributedLockIdentifier lockIdentifier,
            DistributedLockTimeToLive lockTimeToLive,
            IRetryPolicyProvider retryPolicyProvider,
            CancellationToken cancellationToken);

        Task ExtendAsync(
            IDistributedLock distributedLock,
            DistributedLockTimeToLive lockTimeToLive,
            IRetryPolicyProvider retryPolicyProvider,
            CancellationToken cancellationToken);

        Task ReleaseAsync(
            IDistributedLock distributedLock,
            IRetryPolicyProvider retryPolicyProvider,
            CancellationToken cancellationToken);
    }
}