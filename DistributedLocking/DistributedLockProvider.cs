using System;
using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.Abstractions;
using DistributedLocking.Abstractions.Repositories;

namespace DistributedLocking
{
    /// <inheritdoc />
    public class DistributedLockProvider : IDistributedLockProvider
    {
        private readonly IDistributedLocksRepository _repository;

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="repository">
        /// The <see cref="IDistributedLocksRepository"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="repository"/> is null.
        /// </exception>
        public DistributedLockProvider(IDistributedLocksRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }
        
        /// <inheritdoc />
        public async Task<IDistributedLock> TryAcquireAsync(
            DistributedLockResourceId resourceId,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken) =>
            await DistributedLock.TryAcquireAsync(
                    resourceId,
                    timeToLive,
                    _repository,
                    cancellationToken)
                .ConfigureAwait(false);
    }
}