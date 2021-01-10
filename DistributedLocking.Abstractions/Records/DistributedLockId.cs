using System;

namespace DistributedLocking.Abstractions.Records
{
    public record DistributedLockId
    {
        public Guid Value { get; }

        public DistributedLockId(Guid value)
        {
            if (value == Guid.Empty)
            {
                throw new ArgumentException("DistributedLockId cannot be empty guid!", nameof(value));
            }

            Value = value;
        }
        
        public static implicit operator Guid(DistributedLockId id) => id.Value;
        public static implicit operator DistributedLockId(Guid id) => new(id);
    }
}