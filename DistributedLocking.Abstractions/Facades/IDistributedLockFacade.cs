using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.Abstractions.Records;

namespace DistributedLocking.Abstractions.Facades
{
    public interface IDistributedLockFacade
    {
        Task<IDistributedLock> AcquireAsync(
            DistributedLockIdentifier identifier,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken);
    }
}