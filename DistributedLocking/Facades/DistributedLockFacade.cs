using System;
using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.Abstractions;
using DistributedLocking.Abstractions.Facades;
using DistributedLocking.Abstractions.Facades.Exceptions;
using DistributedLocking.Abstractions.Repositories;
using DistributedLocking.Extensions;

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

            var lockId = Guid.NewGuid();

            var success = await _repository.TryInsertIfIdentifierNotExistsAsync(
                    identifier,
                    lockId,
                    timeToLive,
                    cancellationToken)
                .ConfigureAwait(false);

            if (!success)
            {
                throw new CouldNotAcquireLockException(identifier, timeToLive);
            }

            return new DistributedLock(
                identifier,
                lockId,
                _repository);
        }

        public async Task ExtendAsync(
            IDistributedLock distributedLock, 
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken)
        {
            var success = await _repository.TryExtendAsync(
                    distributedLock.Identifier,
                    distributedLock.Id,
                    timeToLive,
                    cancellationToken)
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
            var success = await _repository.TryReleaseAsync(
                    distributedLock.Identifier,
                    distributedLock.Id,
                    cancellationToken)
                .ConfigureAwait(false);

            if (!success)
            {
                throw new CouldNotReleaseLockException(distributedLock.Identifier, distributedLock.Id);
            }
        }
    }
}