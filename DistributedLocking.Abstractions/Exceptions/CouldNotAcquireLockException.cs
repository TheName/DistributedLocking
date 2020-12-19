using System;
using TheName.DistributedLocking.Abstractions.Records;

namespace TheName.DistributedLocking.Abstractions.Exceptions
{
    public class CouldNotAcquireLockException : Exception
    {
        public DistributedLockIdentifier LockIdentifier { get; }
        public DistributedLockTimeout LockTimeout { get; }

        public CouldNotAcquireLockException(
            DistributedLockIdentifier lockIdentifier,
            DistributedLockTimeout lockTimeout) : base(CreateExceptionMessage(lockIdentifier, lockTimeout))
        {
            LockIdentifier = lockIdentifier ?? throw new ArgumentNullException(nameof(lockIdentifier));
            LockTimeout = lockTimeout ?? throw new ArgumentNullException(nameof(lockTimeout));
        }

        private static string CreateExceptionMessage(DistributedLockIdentifier lockIdentifier, DistributedLockTimeout lockTimeout) =>
            $"Could not acquire lock with identifier {lockIdentifier ?? throw new ArgumentNullException(nameof(lockIdentifier))} and timeout {lockTimeout ?? throw new ArgumentNullException(nameof(lockTimeout))}";
    }
}