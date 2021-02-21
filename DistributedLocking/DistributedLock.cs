using System;
using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.Abstractions;
using DistributedLocking.Abstractions.Repositories;

namespace DistributedLocking
{
    /// <inheritdoc />
    public class DistributedLock : IDistributedLock
    {
        private readonly IDistributedLocksRepository _distributedLocksRepository;
        private bool _released;

        /// <inheritdoc />
        public DistributedLockResourceId ResourceId { get; }

        /// <inheritdoc />
        public DistributedLockId Id { get; }

        private DistributedLock(
            DistributedLockResourceId resourceId,
            DistributedLockId id,
            IDistributedLocksRepository repository)
        {
            ResourceId = resourceId ?? throw new ArgumentNullException(nameof(resourceId));
            Id = id ?? throw new ArgumentNullException(nameof(id));
            _distributedLocksRepository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <summary>
        /// Tries to acquire a new distributed lock.
        /// </summary>
        /// <param name="resourceId">
        /// The <see cref="DistributedLockResourceId"/>.
        /// </param>
        /// <param name="timeToLive">
        /// The <see cref="DistributedLockTimeToLive"/>.
        /// </param>
        /// <param name="repository">
        /// The <see cref="IDistributedLocksRepository"/>.
        /// </param>
        /// <param name="cancellationToken">
        /// The <see cref="CancellationToken"/>.
        /// </param>
        /// <returns>
        /// The acquired <see cref="DistributedLock"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when provided <paramref name="resourceId"/>, <paramref name="timeToLive"/> or <paramref name="repository"/> is null.
        /// </exception>
        public static async Task<DistributedLock> TryAcquireAsync(
            DistributedLockResourceId resourceId,
            DistributedLockTimeToLive timeToLive,
            IDistributedLocksRepository repository,
            CancellationToken cancellationToken)
        {
            if (resourceId == null)
            {
                throw new ArgumentNullException(nameof(resourceId));
            }

            if (timeToLive == null)
            {
                throw new ArgumentNullException(nameof(timeToLive));
            }

            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }
            
            var lockId = DistributedLockId.NewLockId();
            var success = await repository.TryInsert(
                    resourceId,
                    lockId,
                    timeToLive,
                    cancellationToken)
                .ConfigureAwait(false);

            return success ? new DistributedLock(resourceId, lockId, repository) : null;
        }

        /// <inheritdoc />
        public async Task<bool> TryExtendAsync(DistributedLockTimeToLive timeToLive, CancellationToken cancellationToken) =>
            await _distributedLocksRepository.TryUpdateTimeToLiveAsync(
                    ResourceId,
                    Id,
                    timeToLive,
                    cancellationToken)
                .ConfigureAwait(false);

        /// <inheritdoc />
        public async Task<bool> TryReleaseAsync(CancellationToken cancellationToken)
        {
            var result = await _distributedLocksRepository.TryDelete(
                    ResourceId,
                    Id,
                    cancellationToken)
                .ConfigureAwait(false);

            _released = true;

            return result;
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            if (_released)
            {
                return;
            }
            
            await TryReleaseAsync(CancellationToken.None).ConfigureAwait(false);
        }
    }
}