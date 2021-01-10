using System;
using DistributedLocking.Abstractions.Records;

namespace DistributedLocking.Abstractions.Repositories.Exceptions
{
    public class CouldNotReleaseLockException : Exception
    {
        public DistributedLockIdentifier Identifier { get; }
        public DistributedLockId Id { get; }

        public CouldNotReleaseLockException(
            DistributedLockIdentifier identifier,
            DistributedLockId id,
            Exception innerException = null) 
            : base(CreateExceptionMessage(identifier, id), innerException)
        {
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
            Id = id ?? throw new ArgumentNullException(nameof(id));
        }

        private static string CreateExceptionMessage(
            DistributedLockIdentifier identifier,
            DistributedLockId id) =>
            $"Could not release lock with identifier {identifier} and  ID {id}";
    }
}