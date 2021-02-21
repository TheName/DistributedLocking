using System;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedLocking.Abstractions
{
    /// <summary>
    /// The distributed lock.
    /// </summary>
    public interface IDistributedLock : IAsyncDisposable
    {
        /// <summary>
        /// The distributed lock's <see cref="DistributedLockResourceId"/>.
        /// </summary>
        DistributedLockResourceId ResourceId { get; }
        
        /// <summary>
        /// The distributed lock's <see cref="DistributedLockId"/>.
        /// </summary>
        DistributedLockId Id { get; }

        /// <summary>
        /// Tries to extend the distributed lock for provided <paramref name="timeToLive"/> period.
        /// <remarks>
        /// Successful extension can only happen if the lock is still active (the previously provided TTL has not yet expired).
        /// </remarks>
        /// </summary>
        /// <param name="timeToLive">
        /// The period for which the distributed lock should be active.
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
        /// - false, if lock was not active (TTL has expired).
        /// </returns>
        Task<bool> TryExtendAsync(DistributedLockTimeToLive timeToLive, CancellationToken cancellationToken);
    }
}