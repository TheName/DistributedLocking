using System;
using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.Abstractions;
using DistributedLocking.Exceptions;

namespace DistributedLocking.Extensions
{
    /// <summary>
    /// The <see cref="IDistributedLockFacade"/> extensions.
    /// </summary>
    public static class DistributedLockFacadeExtensions
    {
        /// <summary>
        /// Acquires a distributed lock or throws if acquiring did not succeed.
        /// </summary>
        /// <param name="facade">
        /// The <see cref="IDistributedLockFacade"/>.
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
        /// The <see cref="Task{IDistributedLock}"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="facade"/> is null.
        /// </exception>
        /// <exception cref="CouldNotAcquireDistributedLockException">
        /// Thrown when acquiring failed.
        /// </exception>
        public static async Task<IDistributedLock> AcquireAsync(
            this IDistributedLockFacade facade,
            DistributedLockResourceId resourceId,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken)
        {
            if (facade == null)
            {
                throw new ArgumentNullException(nameof(facade));
            }
            
            var (success, distributedLock) = await facade.TryAcquireAsync(resourceId, timeToLive, cancellationToken)
                .ConfigureAwait(false);

            if (!success)
            {
                throw new CouldNotAcquireDistributedLockException(resourceId, timeToLive);
            }

            return distributedLock;
        }

        /// <summary>
        /// Extends distributed lock with provided <paramref name="resourceId"/> and <paramref name="id"/> or throws.
        /// </summary>
        /// <param name="facade">
        /// The <see cref="IDistributedLockFacade"/>.
        /// </param>
        /// <param name="resourceId">
        /// The <see cref="DistributedLockResourceId"/>.
        /// </param>
        /// <param name="id">
        /// The <see cref="DistributedLockId"/>.
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
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="facade"/> is null.
        /// </exception>
        /// <exception cref="CouldNotExtendDistributedLockException">
        /// Thrown when extending failed.
        /// </exception>
        public static async Task ExtendAsync(
            this IDistributedLockFacade facade,
            DistributedLockResourceId resourceId,
            DistributedLockId id,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken)
        {
            if (facade == null)
            {
                throw new ArgumentNullException(nameof(facade));
            }

            var success = await facade.TryExtendAsync(
                    resourceId,
                    id,
                    timeToLive,
                    cancellationToken)
                .ConfigureAwait(false);

            if (!success)
            {
                throw new CouldNotExtendDistributedLockException(resourceId, id, timeToLive);
            }
        }

        /// <summary>
        /// Releases distributed lock with provided <paramref name="resourceId"/> and <paramref name="id"/> or throws.
        /// </summary>
        /// <param name="facade">
        /// The <see cref="IDistributedLockFacade"/>.
        /// </param>
        /// <param name="resourceId">
        /// The <see cref="DistributedLockResourceId"/>.
        /// </param>
        /// <param name="id">
        /// The <see cref="DistributedLockId"/>.
        /// </param>
        /// <param name="cancellationToken">
        /// The <see cref="CancellationToken"/>.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="facade"/> is null.
        /// </exception>
        /// <exception cref="CouldNotReleaseDistributedLockException">
        /// Thrown when releasing failed.
        /// </exception>
        public static async Task ReleaseAsync(
            this IDistributedLockFacade facade,
            DistributedLockResourceId resourceId,
            DistributedLockId id,
            CancellationToken cancellationToken)
        {
            if (facade == null)
            {
                throw new ArgumentNullException(nameof(facade));
            }

            var success = await facade.TryReleaseAsync(resourceId, id, cancellationToken).ConfigureAwait(false);

            if (!success)
            {
                throw new CouldNotReleaseDistributedLockException(resourceId, id);
            }
        }
    }
}