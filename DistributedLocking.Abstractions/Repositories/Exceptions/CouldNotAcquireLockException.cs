using System;
using DistributedLocking.Abstractions.Records;

namespace DistributedLocking.Abstractions.Repositories.Exceptions
{
    public class CouldNotAcquireLockException : Exception
    {
        public DistributedLockIdentifier Identifier { get; }
        public DistributedLockTimeToLive TimeToLive { get; }

        public CouldNotAcquireLockException(
            DistributedLockIdentifier identifier,
            DistributedLockTimeToLive timeToLive,
            Exception innerException = null)
            : base(CreateExceptionMessage(identifier, timeToLive), innerException)
        {
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
            TimeToLive = timeToLive ?? throw new ArgumentNullException(nameof(timeToLive));
        }

        private static string CreateExceptionMessage(
            DistributedLockIdentifier identifier,
            DistributedLockTimeToLive timeToLive) =>
            $"Could not acquire lock with identifier {identifier} and time to live {timeToLive}.";
    }
}