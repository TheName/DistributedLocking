using System;
using DistributedLocking.Abstractions.Records;

namespace DistributedLocking.Abstractions.Exceptions
{
    public class CouldNotAcquireLockException : Exception
    {
        public DistributedLockIdentifier LockIdentifier { get; }
        public DistributedLockTimeToLive LockTimeToLive { get; }

        public CouldNotAcquireLockException(
            DistributedLockIdentifier lockIdentifier,
            DistributedLockTimeToLive lockTimeToLive,
            Exception innerException = null)
            : base(CreateExceptionMessage(lockIdentifier, lockTimeToLive), innerException)
        {
            LockIdentifier = lockIdentifier ?? throw new ArgumentNullException(nameof(lockIdentifier));
            LockTimeToLive = lockTimeToLive ?? throw new ArgumentNullException(nameof(lockTimeToLive));
        }

        private static string CreateExceptionMessage(
            DistributedLockIdentifier lockIdentifier,
            DistributedLockTimeToLive lockTimeToLive) =>
            $"Could not acquire lock with identifier {lockIdentifier} and time to live {lockTimeToLive}.";
    }
}