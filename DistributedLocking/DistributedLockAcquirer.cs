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
            if (!await _repository.TryAcquireAsync(lockIdentifier, lockTimeout, out var lockId, cancellationToken).ConfigureAwait(false))
            {
                throw new CouldNotAcquireLockException(lockIdentifier, lockTimeout);
            }

            return new DistributedLock(lockId, _repository);
        }
    }
}