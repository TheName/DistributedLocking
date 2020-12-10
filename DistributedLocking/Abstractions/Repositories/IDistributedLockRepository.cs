using System.Threading;
using System.Threading.Tasks;
using TheName.DistributedLocking.Abstractions.Records;

namespace TheName.DistributedLocking.Abstractions.Repositories
{
    public interface IDistributedLockRepository
    {
        Task<(bool Success, LockId AcquiredLockId)> TryAcquireAsync(
            LockIdentifier lockIdentifier,
            LockTimeout lockTimeout,
            CancellationToken cancellationToken);

        Task<bool> TryReleaseAsync(LockId lockId, CancellationToken cancellationToken);
    }
}