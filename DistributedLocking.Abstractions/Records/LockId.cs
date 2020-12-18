using System;

namespace TheName.DistributedLocking.Abstractions.Records
{
    public record LockId
    {
        public Guid Value { get; }

        public LockId(Guid value)
        {
            if (value == Guid.Empty)
            {
                throw new ArgumentException("Lock ID cannot be empty guid!", nameof(value));
            }

            Value = value;
        }
    }
}