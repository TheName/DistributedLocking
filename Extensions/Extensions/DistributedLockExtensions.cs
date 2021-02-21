using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.Abstractions;
using DistributedLocking.Exceptions;

namespace DistributedLocking.Extensions
{
    /// <summary>
    /// Extensions for <see cref="IDistributedLock"/>.
    /// </summary>
    public static class DistributedLockExtensions
    {
        /// <summary>
        /// Extend distributed lock or throw an exception.
        /// </summary>
        /// <param name="distributedLock">
        /// The <see cref="IDistributedLock"/>.
        /// </param>
        /// <param name="timeToLive">
        /// The <see cref="DistributedLockTimeToLive"/>.
        /// </param>
        /// <param name="cancellationToken">
        /// The <see cref="CancellationToken"/>.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="CouldNotExtendDistributedLockException">
        /// Thrown when extending fails.
        /// </exception>
        public static async Task ExtendAsync(
            this IDistributedLock distributedLock,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken)
        {
            var result = await distributedLock.TryExtendAsync(timeToLive, cancellationToken).ConfigureAwait(false);

            if (!result)
            {
                throw new CouldNotExtendDistributedLockException(
                    distributedLock.ResourceId,
                    distributedLock.Id,
                    timeToLive);
            }
        }

        /// <summary>
        /// Release distributed lock or throw an exception.
        /// </summary>
        /// <param name="distributedLock">
        /// The <see cref="IDistributedLock"/>.
        /// </param>
        /// <param name="cancellationToken">
        /// The <see cref="CancellationToken"/>.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public static async Task ReleaseAsync(
            this IDistributedLock distributedLock,
            CancellationToken cancellationToken)
        {
            var result = await distributedLock.TryReleaseAsync(cancellationToken).ConfigureAwait(false);

            if (!result)
            {
                throw new CouldNotReleaseDistributedLockException(distributedLock.ResourceId, distributedLock.Id);
            }
        }
    }
}