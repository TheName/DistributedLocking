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
        public LockId LockId { get; }
        
        private readonly IDistributedLockRepository _distributedLockRepository;

        public DistributedLock(
            LockId lockId,
            IDistributedLockRepository distributedLockRepository)
        {
            LockId = lockId ?? throw new ArgumentNullException(nameof(lockId));
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