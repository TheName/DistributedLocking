using System.Threading;
using System.Threading.Tasks;

namespace DistributedLocking.Abstractions.Repositories
{
    /// <summary>
    /// The distributed lock repository.
    /// <remarks>
    /// This repository stores distributed locks with their TTLs and ensures that as soon as TTL expires, the locks are no longer active.
    /// </remarks>
    /// </summary>
    public interface IDistributedLocksRepository
    {
        /// <summary>
        /// Tries to insert resource id and lock id for provided TTL period.
        /// <remarks>
        /// Insert is successful only if provided resource id does not have any active (TTL not expired) lock id assigned to it.
        /// </remarks>
        /// </summary>
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
        /// True if insert was successful, false otherwise.
        /// </returns>
        Task<bool> TryInsert(
            DistributedLockResourceId resourceId,
            DistributedLockId id,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken);

        /// <summary>
        /// Tries to update distributed lock's TTL with provided value.
        /// <remarks>
        /// Update is successful only if the lock with provided <paramref name="resourceId"/> and <paramref name="id"/> is still active (TTL has not expired).
        /// </remarks>
        /// </summary>
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
        /// True if update was successful, false otherwise.
        /// </returns>
        Task<bool> TryUpdateTimeToLiveAsync(
            DistributedLockResourceId resourceId,
            DistributedLockId id,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken);

        /// <summary>
        /// Tries to delete distributed lock.
        /// <remarks>
        /// Deletion is successful only if the lock with provided <paramref name="resourceId"/> and <paramref name="id"/> is still active (TTL has not expired).
        /// </remarks>
        /// </summary>
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
        /// True if deletion was successful, false otherwise.
        /// </returns>
        Task<bool> TryDelete(
            DistributedLockResourceId resourceId,
            DistributedLockId id,
            CancellationToken cancellationToken);
    }
}