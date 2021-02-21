using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.Abstractions;
using DistributedLocking.Exceptions;

namespace DistributedLocking.Extensions
{
    /// <summary>
    /// Extensions for <see cref="IDistributedLockProvider"/>.
    /// </summary>
    public static class DistributedLockProviderExtensions
    {
        /// <summary>
        /// Acquires <see cref="IDistributedLock"/> or throws.
        /// </summary>
        /// <param name="distributedLockProvider">
        /// The <see cref="IDistributedLockProvider"/>.
        /// </param>
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
        /// <exception cref="CouldNotAcquireDistributedLockException">
        /// Thrown when acquiring fails.
        /// </exception>
        public static async Task<IDistributedLock> AcquireAsync(
            this IDistributedLockProvider distributedLockProvider,
            DistributedLockResourceId resourceId,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken)
        {
            var result = await distributedLockProvider.TryAcquireAsync(
                    resourceId,
                    timeToLive,
                    cancellationToken)
                .ConfigureAwait(false);

            if (result == null)
            {
                throw new CouldNotAcquireDistributedLockException(resourceId, timeToLive);
            }

            return result;
        }
    }
}