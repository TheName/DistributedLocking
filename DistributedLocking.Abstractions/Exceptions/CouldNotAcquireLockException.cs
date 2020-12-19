using System;
using TheName.DistributedLocking.Abstractions.Records;

namespace TheName.DistributedLocking.Abstractions.Exceptions
{
    public class CouldNotAcquireLockException : Exception
    {
        public DistributedLockIdentifier LockIdentifier { get; }
        public DistributedLockTimeToLive LockTimeToLive { get; }
        public DistributedLockAcquiringTimeout AcquiringTimeout { get; }

        public CouldNotAcquireLockException(
            DistributedLockIdentifier lockIdentifier,
            DistributedLockTimeToLive lockTimeToLive,
            DistributedLockAcquiringTimeout acquiringTimeout)
            : base(CreateExceptionMessage(lockIdentifier, lockTimeToLive, acquiringTimeout))
        {
            LockIdentifier = lockIdentifier ?? throw new ArgumentNullException(nameof(lockIdentifier));
            LockTimeToLive = lockTimeToLive ?? throw new ArgumentNullException(nameof(lockTimeToLive));
            AcquiringTimeout = acquiringTimeout ?? throw new ArgumentNullException(nameof(acquiringTimeout));
        }

        private static string CreateExceptionMessage(
            DistributedLockIdentifier lockIdentifier,
            DistributedLockTimeToLive lockTimeToLive,
            DistributedLockAcquiringTimeout acquiringTimeout) =>
            $"Could not acquire lock with identifier {lockIdentifier} and time to live {lockTimeToLive} with acquiring timeout of {acquiringTimeout}";
    }
}