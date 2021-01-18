using System;

namespace DistributedLocking.Abstractions
{
    public sealed class DistributedLockIdentifier
    {
        public Guid Value { get; }

        public DistributedLockIdentifier(Guid value)
        {
            if (value == Guid.Empty)
            {
                throw new ArgumentException($"{nameof(DistributedLockIdentifier)} cannot be empty guid!", nameof(value));
            }
            
            Value = value;
        }

        #region Operators
        
        public static implicit operator Guid(DistributedLockIdentifier identifier) => 
            identifier.Value;
        
        public static implicit operator DistributedLockIdentifier(Guid identifier) =>
            new DistributedLockIdentifier(identifier);

        public static bool operator ==(DistributedLockIdentifier lockIdentifier, DistributedLockIdentifier otherLockIdentifier) =>
            Equals(lockIdentifier, otherLockIdentifier);

        public static bool operator !=(DistributedLockIdentifier lockIdentifier, DistributedLockIdentifier otherLockIdentifier) =>
            !(lockIdentifier == otherLockIdentifier);

        #endregion

        public override bool Equals(object obj) =>
            obj is DistributedLockIdentifier other &&
            other.GetHashCode() == Value.GetHashCode();

        public override int GetHashCode() =>
            Value.GetHashCode();

        public override string ToString() => 
            Value.ToString();
    }
}