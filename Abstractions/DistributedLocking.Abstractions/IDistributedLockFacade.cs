using System.Threading;
using System.Threading.Tasks;

namespace DistributedLocking.Abstractions
{
    /// <summary>
    /// The facade to interact with acquire and interact with <see cref="IDistributedLock"/>.
    /// </summary>
    public interface IDistributedLockFacade
    {
        /// <summary>
        /// Tries to acquire a distributed lock with provided <paramref name="identifier"/> for a period defined by <paramref name="timeToLive"/>. 
        /// </summary>
        /// <param name="identifier">
        /// The unique identifier of <see cref="IDistributedLock"/>.
        /// </param>
        /// <param name="timeToLive">
        /// The period for which the provided <paramref name="identifier"/> should be locked if acquiring succeeds.
        /// </param>
        /// <param name="cancellationToken">
        /// The <see cref="CancellationToken"/>.
        /// </param>
        /// <returns>
        /// A tuple containing:
        ///     - <see cref="bool"/> defining if acquiring succeeded.
        ///     - acquired <see cref="IDistributedLock"/> if acquiring succeeded (null otherwise).
        /// </returns>
        Task<(bool Success, IDistributedLock distributedLock)> TryAcquireAsync(
            DistributedLockIdentifier identifier,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken);
        
        /// <summary>
        /// Tries to extend already acquired lock defined by <paramref name="identifier"/> and <paramref name="id"/> for provided <paramref name="timeToLive"/> period.
        /// <remarks>
        /// Successful extension can only happen if the lock is still active (the previously provided TTL has not yet expired).
        /// </remarks>
        /// </summary>
        /// <param name="identifier">
        /// The unique identifier of <see cref="IDistributedLock"/>.
        /// </param>
        /// <param name="id">
        /// Acquired lock's <see cref="DistributedLockId"/>.
        /// </param>
        /// <param name="timeToLive">
        /// The period for which the provided <paramref name="identifier"/> and <paramref name="id"/> should be locked if extending succeeds.
        /// <remarks>
        /// The provided period does NOT extend already existing TTL but replaces it.
        /// </remarks>
        /// </param>
        /// <param name="cancellationToken">
        /// The <see cref="CancellationToken"/>.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/> defining if action has succeeded.
        /// - true, if lock was still active
        /// - false, if lock was not active (either does not exist at all, or its TTL has expired).
        /// </returns>
        Task<bool> TryExtendAsync(
            DistributedLockIdentifier identifier,
            DistributedLockId id,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken);

        /// <summary>
        /// Tries to release already acquired lock defined by <paramref name="identifier"/> and <paramref name="id"/>.
        /// <remarks>
        /// Successful release can only happen if the lock is still active (the previously provided TTL has not yet expired).
        /// </remarks>
        /// </summary>
        /// <param name="identifier">
        /// The unique identifier of <see cref="IDistributedLock"/>.
        /// </param>
        /// <param name="id">
        /// Acquired lock's <see cref="DistributedLockId"/>.
        /// </param>
        /// <param name="cancellationToken">
        /// The <see cref="CancellationToken"/>.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/> defining if action has succeeded.
        /// - true, if lock was still active and thus releasing was successful.
        /// - false, if lock was not active (either does not exist at all, or its TTL has expired) and thus releasing was unsuccessful, because the lock was already released due to TTL expiration.
        /// </returns>
        Task<bool> TryReleaseAsync(
            DistributedLockIdentifier identifier,
            DistributedLockId id,
            CancellationToken cancellationToken);
    }
}