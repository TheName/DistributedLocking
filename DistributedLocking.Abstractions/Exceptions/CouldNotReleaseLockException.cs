using System;
using DistributedLocking.Abstractions.Records;

namespace DistributedLocking.Abstractions.Exceptions
{
    public class CouldNotReleaseLockException : Exception
    {
        public DistributedLockIdentifier LockIdentifier { get; }
        public DistributedLockId LockId { get; }

        public CouldNotReleaseLockException(
            DistributedLockIdentifier lockIdentifier,
            DistributedLockId lockId) 
            : base(CreateExceptionMessage(lockIdentifier, lockId))
        {
            LockIdentifier = lockIdentifier ?? throw new ArgumentNullException(nameof(lockIdentifier));
            LockId = lockId ?? throw new ArgumentNullException(nameof(lockId));
        }

        private static string CreateExceptionMessage(
            DistributedLockIdentifier lockIdentifier,
            DistributedLockId lockId) =>
            $"Could not release lock with identifier {lockIdentifier} and  ID {lockId}";
    }
}