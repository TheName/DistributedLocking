using System;

namespace DistributedLocking.Abstractions.Records
{
    public record DistributedLockIdentifier
    {
        public Guid Value { get; }

        public DistributedLockIdentifier(Guid value)
        {
            if (value == Guid.Empty)
            {
                throw new ArgumentException($"{nameof(DistributedLockIdentifier)} cannot be empty guid!", nameof(value));
            }
            
            Value = value;
        }
        
        public static implicit operator Guid(DistributedLockIdentifier identifier) => identifier.Value;
        public static implicit operator DistributedLockIdentifier(Guid identifier) => new(identifier);
    }
}