using System;

namespace TheName.DistributedLocking.Abstractions.Records
{
    public record DistributedLockId
    {
        public Guid Value { get; }

        public DistributedLockId(Guid value)
        {
            if (value == Guid.Empty)
            {
                throw new ArgumentException("DistributedLockId ID cannot be empty guid!", nameof(value));
            }

            Value = value;
        }
    }
}