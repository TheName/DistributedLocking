using System;
using TheName.DistributedLocking.Abstractions.Records;

namespace TheName.DistributedLocking.Abstractions.Exceptions
{
    public class CouldNotExtendLockException : Exception
    {
        public DistributedLockIdentifier LockIdentifier { get; }
        public DistributedLockId LockId { get; }

        public CouldNotExtendLockException(
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
            $"Could not extend lock with identifier {lockIdentifier} and  ID {lockId}";
    }
}