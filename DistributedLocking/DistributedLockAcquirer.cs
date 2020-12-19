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
            DistributedLockIdentifier lockIdentifier,
            DistributedLockTimeToLive lockTimeToLive,
            CancellationToken cancellationToken)
        {
            var (success, acquiredLockId) = await _repository.TryAcquireAsync(lockIdentifier, lockTimeToLive, cancellationToken).ConfigureAwait(false); 
            if (!success)
            {
                throw new CouldNotAcquireLockException(lockIdentifier, lockTimeToLive);
            }

            return new DistributedLock(acquiredLockId, lockIdentifier, _repository);
        }
    }
}