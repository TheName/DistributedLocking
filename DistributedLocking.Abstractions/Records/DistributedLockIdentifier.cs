using System;

namespace TheName.DistributedLocking.Abstractions.Records
{
    public record DistributedLockIdentifier
    {
        public Guid Value { get; }

        public DistributedLockIdentifier(Guid value)
        {
            if (value == Guid.Empty)
            {
                throw new ArgumentException("DistributedLockIdentifier identifier cannot be empty guid!", nameof(value));
            }
            
            Value = value;
        }
        
        public static implicit operator Guid(DistributedLockIdentifier lockIdentifier) => lockIdentifier.Value;
        public static implicit operator DistributedLockIdentifier(Guid lockIdentifier) => new(lockIdentifier);
    }
}