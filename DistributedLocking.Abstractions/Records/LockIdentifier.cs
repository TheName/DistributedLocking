using System;

namespace TheName.DistributedLocking.Abstractions.Records
{
    public record LockIdentifier
    {
        public Guid Value { get; }

        public LockIdentifier(Guid value)
        {
            if (value == Guid.Empty)
            {
                throw new ArgumentException("Lock identifier cannot be empty guid!", nameof(value));
            }
            
            Value = value;
        }
    }
}