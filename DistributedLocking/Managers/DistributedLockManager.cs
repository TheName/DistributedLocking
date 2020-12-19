using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using TheName.DistributedLocking.Abstractions;
using TheName.DistributedLocking.Abstractions.Exceptions;
using TheName.DistributedLocking.Abstractions.Managers;
using TheName.DistributedLocking.Abstractions.Records;
using TheName.DistributedLocking.Abstractions.Repositories;

namespace TheName.DistributedLocking.Managers
{
    public class DistributedLockManager : IDistributedLockManager
    {
        private readonly IDistributedLockRepository _repository;

        public DistributedLockManager(IDistributedLockRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }
        
        public async Task<IDistributedLock> AcquireAsync(
            DistributedLockIdentifier lockIdentifier,
            DistributedLockTimeToLive lockTimeToLive,
            DistributedLockAcquiringTimeout acquiringTimeout,
            DistributedLockAcquiringDelayBetweenRetries delayBetweenRetries,
            CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            do
            {
                var (success, acquiredLockId) = await _repository.TryAcquireAsync(
                        lockIdentifier,
                        lockTimeToLive,
                        cancellationToken)
                    .ConfigureAwait(false);
                
                if (success)
                {
                    return new DistributedLock(
                        acquiredLockId,
                        lockIdentifier,
                        _repository);
                }

                await Task.Delay(delayBetweenRetries.Value, cancellationToken).ConfigureAwait(false);
            } while (stopwatch.Elapsed < acquiringTimeout.Value);

            throw new CouldNotAcquireLockException(lockIdentifier, lockTimeToLive, acquiringTimeout);
        }

        public async Task ExtendAsync(
            IDistributedLock distributedLock, 
            DistributedLockTimeToLive lockTimeToLive,
            CancellationToken cancellationToken)
        {
            var result = await _repository.TryExtendAsync(
                    distributedLock.LockIdentifier,
                    distributedLock.LockId,
                    lockTimeToLive,
                    cancellationToken)
                .ConfigureAwait(false);
            
            if (!result)
            {
                throw new CouldNotExtendLockException(distributedLock.LockIdentifier, distributedLock.LockId);
            }
        }

        public async Task ReleaseAsync(IDistributedLock distributedLock, CancellationToken cancellationToken)
        {
            var result = await _repository.TryReleaseAsync(
                    distributedLock.LockIdentifier,
                    distributedLock.LockId,
                    cancellationToken)
                .ConfigureAwait(false);
            
            if (!result)
            {
                throw new CouldNotReleaseLockException(distributedLock.LockIdentifier, distributedLock.LockId);
            }
        }
    }
}