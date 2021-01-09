using System;

namespace DistributedLocking.Abstractions.Records
{
    public record DistributedLockTimeToLive
    {
        public TimeSpan Value { get; }

        public DistributedLockTimeToLive(TimeSpan value)
        {
            if (value <= TimeSpan.Zero)
            {
                throw new ArgumentException("DistributedLockTimeToLive cannot be lower than or equal to zero!", nameof(value));
            }
            
            Value = value;
        }
        
        public static implicit operator TimeSpan(DistributedLockTimeToLive timeToLive) => timeToLive.Value;
        public static implicit operator DistributedLockTimeToLive(TimeSpan timeToLive) => new(timeToLive);
    }
}