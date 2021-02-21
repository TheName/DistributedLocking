using System;
using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.Abstractions;
using DistributedLocking.Abstractions.Repositories;

namespace DistributedLocking
{
    /// <inheritdoc />
    public class DistributedLockFacade : IDistributedLockFacade
    {
        private readonly IDistributedLocksRepository _repository;

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="repository">
        /// The <see cref="IDistributedLocksRepository"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when provided <paramref name="repository"/> is null.
        /// </exception>
        public DistributedLockFacade(IDistributedLocksRepository repository)
        {
            _repository = EnsureIsNotNull(repository, nameof(repository));
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">
        /// Thrown when provided <paramref name="resourceId"/> or <paramref name="timeToLive"/> is null.
        /// </exception>
        public async Task<(bool Success, IDistributedLock distributedLock)> TryAcquireAsync(
            DistributedLockResourceId resourceId,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken)
        {
            EnsureIsNotNull(resourceId, nameof(resourceId));
            EnsureIsNotNull(timeToLive, nameof(timeToLive));

            var lockId = Guid.NewGuid();

            var success = await _repository.TryInsert(
                    resourceId,
                    lockId,
                    timeToLive,
                    cancellationToken)
                .ConfigureAwait(false);

            var @lock = success ? new DistributedLock(resourceId, lockId, this) : null;

            return (success, @lock);
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">
        /// Thrown when provided <paramref name="resourceId"/>, <paramref name="id"/> or <paramref name="timeToLive"/> is null.
        /// </exception>
        public async Task<bool> TryExtendAsync(
            DistributedLockResourceId resourceId,
            DistributedLockId id,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken)
        {
            EnsureIsNotNull(resourceId, nameof(resourceId));
            EnsureIsNotNull(id, nameof(id));
            EnsureIsNotNull(timeToLive, nameof(timeToLive));
            
            return await _repository.TryUpdateTimeToLiveAsync(
                    resourceId,
                    id,
                    timeToLive,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">
        /// Thrown when provided <paramref name="resourceId"/> or <paramref name="id"/> is null.
        /// </exception>
        public async Task<bool> TryReleaseAsync(
            DistributedLockResourceId resourceId,
            DistributedLockId id,
            CancellationToken cancellationToken)
        {
            EnsureIsNotNull(resourceId, nameof(resourceId));
            EnsureIsNotNull(id, nameof(id));
            
            return await _repository.TryDelete(
                    resourceId,
                    id,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        private static T EnsureIsNotNull<T>(T instance, string name)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(name);
            }

            return instance;
        }
    }
}