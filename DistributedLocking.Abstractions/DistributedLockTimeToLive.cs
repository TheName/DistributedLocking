using System;

namespace DistributedLocking.Abstractions
{
    public sealed class DistributedLockTimeToLive
    {
        public TimeSpan Value { get; }

        public DistributedLockTimeToLive(TimeSpan value)
        {
            if (value <= TimeSpan.Zero)
            {
                throw new ArgumentException($"{nameof(DistributedLockTimeToLive)} cannot be lower than or equal to zero!", nameof(value));
            }
            
            Value = value;
        }

        #region Operators
        
        public static implicit operator TimeSpan(DistributedLockTimeToLive timeToLive) =>
            timeToLive.Value;
        
        public static implicit operator DistributedLockTimeToLive(TimeSpan timeToLive) => 
            new DistributedLockTimeToLive(timeToLive);

        public static bool operator ==(DistributedLockTimeToLive timeToLive, DistributedLockTimeToLive otherTimeToLive) =>
            Equals(timeToLive, otherTimeToLive);

        public static bool operator !=(DistributedLockTimeToLive timeToLive, DistributedLockTimeToLive otherTimeToLive) =>
            !(timeToLive == otherTimeToLive);

        #endregion

        public override bool Equals(object obj) =>
            obj is DistributedLockTimeToLive other &&
            other.GetHashCode() == Value.GetHashCode();

        public override int GetHashCode() =>
            Value.GetHashCode();

        public override string ToString() => 
            Value.ToString();
    }
}