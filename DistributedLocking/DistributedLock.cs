using System;
using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.Abstractions;

namespace DistributedLocking
{
    /// <inheritdoc />
    public class DistributedLock : IDistributedLock
    {
        private readonly IDistributedLockFacade _distributedLockFacade;

        /// <inheritdoc />
        public DistributedLockResourceId ResourceId { get; }

        /// <inheritdoc />
        public DistributedLockId Id { get; }

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="resourceId">
        /// The <see cref="DistributedLockResourceId"/>.
        /// </param>
        /// <param name="id">
        /// The <see cref="DistributedLockId"/>.
        /// </param>
        /// <param name="distributedLockFacade">
        /// The <see cref="IDistributedLockFacade"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when provided <paramref name="resourceId"/>, <paramref name="id"/> or <paramref name="distributedLockFacade"/> is null.
        /// </exception>
        public DistributedLock(
            DistributedLockResourceId resourceId,
            DistributedLockId id,
            IDistributedLockFacade distributedLockFacade)
        {
            ResourceId = resourceId ?? throw new ArgumentNullException(nameof(resourceId));
            Id = id ?? throw new ArgumentNullException(nameof(id));
            _distributedLockFacade = distributedLockFacade ?? throw new ArgumentNullException(nameof(distributedLockFacade));
        }

        /// <inheritdoc />
        public async Task<bool> TryExtendAsync(DistributedLockTimeToLive timeToLive, CancellationToken cancellationToken) =>
            await _distributedLockFacade.TryExtendAsync(
                    ResourceId,
                    Id,
                    timeToLive,
                    cancellationToken)
                .ConfigureAwait(false);

        /// <inheritdoc />
        public async ValueTask DisposeAsync() =>
            await _distributedLockFacade.TryReleaseAsync(
                    ResourceId,
                    Id,
                    CancellationToken.None)
                .ConfigureAwait(false);
    }
}