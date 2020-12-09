using System;
using TheName.DistributedLocking.Abstractions.Records;

namespace TheName.DistributedLocking.Abstractions.Exceptions
{
    public class CouldNotAcquireLockException : Exception
    {
        public LockIdentifier LockIdentifier { get; }
        public LockTimeout LockTimeout { get; }

        public CouldNotAcquireLockException(
            LockIdentifier lockIdentifier,
            LockTimeout lockTimeout) : base(CreateExceptionMessage(lockIdentifier, lockTimeout))
        {
            LockIdentifier = lockIdentifier ?? throw new ArgumentNullException(nameof(lockIdentifier));
            LockTimeout = lockTimeout ?? throw new ArgumentNullException(nameof(lockTimeout));
        }

        private static string CreateExceptionMessage(LockIdentifier lockIdentifier, LockTimeout lockTimeout) =>
            $"Could not acquire lock with identifier {lockIdentifier ?? throw new ArgumentNullException(nameof(lockIdentifier))} and timeout {lockTimeout ?? throw new ArgumentNullException(nameof(lockTimeout))}";
    }
}