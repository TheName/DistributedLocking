using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.Abstractions.Records;
using DistributedLocking.Abstractions.Retries;

namespace DistributedLocking.Abstractions.Facades
{
    public interface IDistributedLockFacade
    {
        Task<IDistributedLock> AcquireAsync(
            DistributedLockIdentifier identifier,
            DistributedLockTimeToLive timeToLive,
            IRetryPolicyProvider retryPolicyProvider,
            CancellationToken cancellationToken);

        Task ExtendAsync(
            IDistributedLock distributedLock,
            DistributedLockTimeToLive timeToLive,
            IRetryPolicyProvider retryPolicyProvider,
            CancellationToken cancellationToken);

        Task ReleaseAsync(
            IDistributedLock distributedLock,
            IRetryPolicyProvider retryPolicyProvider,
            CancellationToken cancellationToken);
    }
}