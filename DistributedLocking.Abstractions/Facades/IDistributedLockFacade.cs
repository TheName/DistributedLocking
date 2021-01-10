using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.Abstractions.Retries;

namespace DistributedLocking.Abstractions.Facades
{
    public interface IDistributedLockFacade
    {
        Task<IDistributedLock> AcquireAsync(
            DistributedLockIdentifier identifier,
            DistributedLockTimeToLive timeToLive,
            IRetryPolicy retryPolicy,
            CancellationToken cancellationToken);

        Task ExtendAsync(
            IDistributedLock distributedLock,
            DistributedLockTimeToLive timeToLive,
            IRetryPolicy retryPolicy,
            CancellationToken cancellationToken);

        Task ReleaseAsync(
            IDistributedLock distributedLock,
            IRetryPolicy retryPolicy,
            CancellationToken cancellationToken);
    }
}