using System;
using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.Abstractions;
using DistributedLocking.Abstractions.Records;
using DistributedLocking.Abstractions.Repositories;

namespace DistributedLocking
{
    internal class DistributedLock : IDistributedLock
    {
        public DistributedLockId LockId { get; }
        
        public DistributedLockIdentifier LockIdentifier { get; }

        private readonly IDistributedLockRepository _distributedLockRepository;

        public DistributedLock(
            DistributedLockId lockId,
            DistributedLockIdentifier lockIdentifier,
            IDistributedLockRepository distributedLockRepository)
        {
            LockId = lockId ?? throw new ArgumentNullException(nameof(lockId));
            LockIdentifier = lockIdentifier ?? throw new ArgumentNullException(nameof(lockIdentifier));
            _distributedLockRepository = distributedLockRepository ?? throw new ArgumentNullException(nameof(distributedLockRepository));
        }

        public async ValueTask DisposeAsync()
        {
            await _distributedLockRepository.TryReleaseAsync(
                    LockIdentifier,
                    LockId,
                    CancellationToken.None)
                .ConfigureAwait(false);
        }
    }
}