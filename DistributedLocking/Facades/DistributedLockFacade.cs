using System;
using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.Abstractions;
using DistributedLocking.Abstractions.Facades;
using DistributedLocking.Abstractions.Facades.Exceptions;
using DistributedLocking.Abstractions.Repositories;
using DistributedLocking.Extensions;
using DistributedLocking.Extensions.Abstractions.Repositories;

namespace DistributedLocking.Facades
{
    public class DistributedLockFacade : IDistributedLockFacade
    {
        private readonly IDistributedLockRepository _repository;

        public DistributedLockFacade(IDistributedLockRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }
        
        public async Task<IDistributedLock> AcquireAsync(
            DistributedLockIdentifier identifier,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken)
        {
            identifier.EnsureIsNotNull(nameof(identifier));
            timeToLive.EnsureIsNotNull(nameof(timeToLive));

            var (success, acquiredLock) = await _repository.TryAcquireLockAsync(identifier, timeToLive, cancellationToken)
                .ConfigureAwait(false);

            if (!success)
            {
                throw new CouldNotAcquireLockException(identifier, timeToLive);
            }

            return acquiredLock;
        }

        public async Task ExtendAsync(
            IDistributedLock distributedLock, 
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken)
        {
            var success = await _repository.TryExtendAsync(distributedLock, timeToLive, cancellationToken)
                .ConfigureAwait(false);

            if (!success)
            {
                throw new CouldNotExtendLockException(distributedLock.Identifier, distributedLock.Id);
            }
        }

        public async Task ReleaseAsync(
            IDistributedLock distributedLock,
            CancellationToken cancellationToken)
        {
            var success = await _repository.TryReleaseAsync(distributedLock, cancellationToken)
                .ConfigureAwait(false);

            if (!success)
            {
                throw new CouldNotReleaseLockException(distributedLock.Identifier, distributedLock.Id);
            }
        }
    }
}