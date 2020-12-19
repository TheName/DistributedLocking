using System;

namespace TheName.DistributedLocking.Abstractions.Records
{
    public record DistributedLockAcquiringTimeout
    {
        public TimeSpan Value { get; }

        public DistributedLockAcquiringTimeout(TimeSpan value)
        {
            if (value <= TimeSpan.Zero)
            {
                throw new ArgumentException("DistributedLockAcquiringTimeout cannot be lower than or equal to zero!", nameof(value));
            }
            
            Value = value;
        }
        
        public static implicit operator TimeSpan(DistributedLockAcquiringTimeout timeout) => timeout.Value;
        public static implicit operator DistributedLockAcquiringTimeout(TimeSpan timeout) => new(timeout);
    }
}