using System;

namespace TheName.DistributedLocking.Abstractions.Records
{
    public record DistributedLockAcquiringDelayBetweenRetries
    {
        public TimeSpan Value { get; }

        public DistributedLockAcquiringDelayBetweenRetries(TimeSpan value)
        {
            if (value < TimeSpan.Zero)
            {
                throw new ArgumentException("DistributedLockAcquiringDelayBetweenRetries cannot be lower than zero!", nameof(value));
            }
            
            Value = value;
        }
        
        public static implicit operator TimeSpan(DistributedLockAcquiringDelayBetweenRetries delay) => delay.Value;
        public static implicit operator DistributedLockAcquiringDelayBetweenRetries(TimeSpan delay) => new(delay);
    }
}