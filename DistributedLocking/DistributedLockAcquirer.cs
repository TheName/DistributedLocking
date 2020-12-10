using System;
using System.Threading;
using System.Threading.Tasks;
using TheName.DistributedLocking.Abstractions;
using TheName.DistributedLocking.Abstractions.Exceptions;
using TheName.DistributedLocking.Abstractions.Records;
using TheName.DistributedLocking.Abstractions.Repositories;

namespace TheName.DistributedLocking
{
    public class DistributedLockAcquirer : IDistributedLockAcquirer
    {
        private readonly IDistributedLockRepository _repository;

        public DistributedLockAcquirer(IDistributedLockRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }
        
        public async Task<IDistributedLock> AcquireAsync(
            LockIdentifier lockIdentifier,
            LockTimeout lockTimeout,
            CancellationToken cancellationToken)
        {
            var (success, acquiredLockId) = await _repository.TryAcquireAsync(lockIdentifier, lockTimeout, cancellationToken).ConfigureAwait(false); 
            if (!success)
            {
                throw new CouldNotAcquireLockException(lockIdentifier, lockTimeout);
            }

            return new DistributedLock(acquiredLockId, _repository);
        }
    }
}