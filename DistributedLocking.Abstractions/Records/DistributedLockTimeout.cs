using System;

namespace TheName.DistributedLocking.Abstractions.Records
{
    public record DistributedLockTimeout
    {
        public TimeSpan Value { get; }

        public DistributedLockTimeout(TimeSpan value)
        {
            if (value <= TimeSpan.Zero)
            {
                throw new ArgumentException("DistributedLockTimeout timeout cannot be lower or equal to zero!", nameof(value));
            }
            
            Value = value;
        }
    }
}