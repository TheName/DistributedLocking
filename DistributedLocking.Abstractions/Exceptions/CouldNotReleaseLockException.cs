using System;
using TheName.DistributedLocking.Abstractions.Records;

namespace TheName.DistributedLocking.Abstractions.Exceptions
{
    public class CouldNotReleaseLockException : Exception
    {
        public LockId LockId { get; }

        public CouldNotReleaseLockException(LockId lockId) : base(CreateExceptionMessage(lockId))
        {
            LockId = lockId ?? throw new ArgumentNullException(nameof(lockId));
        }

        private static string CreateExceptionMessage(LockId lockId) =>
            $"Could not release lock with ID {lockId ?? throw new ArgumentNullException(nameof(lockId))}";
    }
}