using System;
using TheName.DistributedLocking.Abstractions.Records;

namespace TheName.DistributedLocking.Abstractions.Exceptions
{
    public class CouldNotExtendLockException : Exception
    {
        public DistributedLockId LockId { get; }

        public CouldNotExtendLockException(DistributedLockId lockId) : base(CreateExceptionMessage(lockId))
        {
            LockId = lockId ?? throw new ArgumentNullException(nameof(lockId));
        }

        private static string CreateExceptionMessage(DistributedLockId lockId) =>
            $"Could not extend lock with ID {lockId}";
    }
}