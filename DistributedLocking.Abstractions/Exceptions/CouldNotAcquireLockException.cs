using System;
using TheName.DistributedLocking.Abstractions.Records;

namespace TheName.DistributedLocking.Abstractions.Exceptions
{
    public class CouldNotAcquireLockException : Exception
    {
        public DistributedLockIdentifier LockIdentifier { get; }
        public DistributedLockTimeToLive LockTimeToLive { get; }

        public CouldNotAcquireLockException(
            DistributedLockIdentifier lockIdentifier,
            DistributedLockTimeToLive lockTimeToLive) : base(CreateExceptionMessage(lockIdentifier, lockTimeToLive))
        {
            LockIdentifier = lockIdentifier ?? throw new ArgumentNullException(nameof(lockIdentifier));
            LockTimeToLive = lockTimeToLive ?? throw new ArgumentNullException(nameof(lockTimeToLive));
        }

        private static string CreateExceptionMessage(DistributedLockIdentifier lockIdentifier, DistributedLockTimeToLive lockTimeToLive) =>
            $"Could not acquire lock with identifier {lockIdentifier ?? throw new ArgumentNullException(nameof(lockIdentifier))} and time to live {lockTimeToLive ?? throw new ArgumentNullException(nameof(lockTimeToLive))}";
    }
}