using System.Threading;
using System.Threading.Tasks;

namespace DistributedLocking.Abstractions.Facades
{
    public interface IDistributedLockFacade
    {
        Task<IDistributedLock> AcquireAsync(
            DistributedLockIdentifier identifier,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken);

        Task ExtendAsync(
            IDistributedLock distributedLock,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken);

        Task ReleaseAsync(
            IDistributedLock distributedLock,
            CancellationToken cancellationToken);
    }
}