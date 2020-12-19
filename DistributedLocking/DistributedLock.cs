using System;
using System.Threading;
using System.Threading.Tasks;
using TheName.DistributedLocking.Abstractions;
using TheName.DistributedLocking.Abstractions.Exceptions;
using TheName.DistributedLocking.Abstractions.Records;
using TheName.DistributedLocking.Abstractions.Repositories;

namespace TheName.DistributedLocking
{
    internal class DistributedLock : IDistributedLock
    {
        public DistributedLockId LockId { get; }
        
        public LockIdentifier LockIdentifier { get; }

        private readonly IDistributedLockRepository _distributedLockRepository;

        public DistributedLock(
            DistributedLockId lockId,
            LockIdentifier lockIdentifier,
            IDistributedLockRepository distributedLockRepository)
        {
            LockId = lockId ?? throw new ArgumentNullException(nameof(lockId));
            LockIdentifier = lockIdentifier ?? throw new ArgumentNullException(nameof(lockIdentifier));
            _distributedLockRepository = distributedLockRepository ?? throw new ArgumentNullException(nameof(distributedLockRepository));
        }

        public async ValueTask DisposeAsync()
        {
            if (!await _distributedLockRepository.TryReleaseAsync(LockId, CancellationToken.None).ConfigureAwait(false))
            {
                throw new CouldNotReleaseLockException(LockId);
            }
        }
    }
}