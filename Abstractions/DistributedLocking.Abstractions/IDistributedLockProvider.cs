using System.Threading;
using System.Threading.Tasks;

namespace DistributedLocking.Abstractions
{
    /// <summary>
    /// Provides <see cref="IDistributedLock"/>.
    /// </summary>
    public interface IDistributedLockProvider
    {
        /// <summary>
        /// Tries to acquire <see cref="IDistributedLock"/>.
        /// </summary>
        /// <param name="resourceId">
        /// The <see cref="DistributedLockResourceId"/>.
        /// </param>
        /// <param name="timeToLive">
        /// The <see cref="DistributedLockTimeToLive"/>.
        /// </param>
        /// <param name="cancellationToken">
        /// The <see cref="CancellationToken"/>.
        /// </param>
        /// <returns>
        /// The <see cref="IDistributedLock"/>.
        /// </returns>
        Task<IDistributedLock> TryAcquireAsync(
            DistributedLockResourceId resourceId,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken);
    }
}