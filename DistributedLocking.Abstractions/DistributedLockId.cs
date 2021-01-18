using System;

namespace DistributedLocking.Abstractions
{
    public sealed class DistributedLockId
    {
        public Guid Value { get; }

        public DistributedLockId(Guid value)
        {
            if (value == Guid.Empty)
            {
                throw new ArgumentException($"{nameof(DistributedLockId)} cannot be empty guid!", nameof(value));
            }

            Value = value;
        }

        #region Operators

        public static implicit operator Guid(DistributedLockId id) => 
            id.Value;
        public static implicit operator DistributedLockId(Guid id) => 
            new DistributedLockId(id);

        public static bool operator ==(DistributedLockId lockId, DistributedLockId otherLockId) =>
            Equals(lockId, otherLockId);

        public static bool operator !=(DistributedLockId lockId, DistributedLockId otherLockId) =>
            !(lockId == otherLockId);

        #endregion

        public override bool Equals(object obj) =>
            obj is DistributedLockId other &&
            other.GetHashCode() == Value.GetHashCode();

        public override int GetHashCode() =>
            Value.GetHashCode();

        public override string ToString() => 
            Value.ToString();
    }
}