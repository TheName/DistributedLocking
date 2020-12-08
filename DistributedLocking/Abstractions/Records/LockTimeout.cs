using System;

namespace TheName.DistributedLocking.Abstractions.Records
{
    public record LockTimeout
    {
        public TimeSpan Value { get; }

        public LockTimeout(TimeSpan value)
        {
            if (value <= TimeSpan.Zero)
            {
                throw new ArgumentException($"Lock timeout cannot be lower or equal to zero!", nameof(value));
            }
            
            Value = value;
        }
    }
}