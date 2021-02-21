using System;

namespace DistributedLocking.Abstractions
{
    /// <summary>
    /// The distributed lock id value object.
    /// <remarks>
    /// Identifies an instance of a distributed lock.
    /// </remarks>
    /// </summary>
    public sealed class DistributedLockId
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DistributedLockId"/>.
        /// </summary>
        /// <returns>
        /// A new <see cref="DistributedLockId"/> object.
        /// </returns>
        public static DistributedLockId NewLockId() => Guid.NewGuid();
        
        /// <summary>
        /// The ID value.
        /// </summary>
        public Guid Value { get; }

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="value">
        /// The id.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown when provided <paramref name="value"/> is an empty <see cref="Guid"/>.
        /// </exception>
        public DistributedLockId(Guid value)
        {
            if (value == Guid.Empty)
            {
                throw new ArgumentException($"{nameof(DistributedLockId)} cannot be empty guid!", nameof(value));
            }

            Value = value;
        }

        #region Operators

        /// <summary>
        /// Implicit operator that converts the <see cref="DistributedLockId"/> to <see cref="Guid"/>.
        /// </summary>
        /// <param name="id">
        /// The <see cref="DistributedLockId"/>.
        /// </param>
        /// <returns>
        /// The <see cref="Guid"/>.
        /// </returns>
        public static implicit operator Guid(DistributedLockId id) => id.Value;
        
        /// <summary>
        /// Implicit operator that converts the <see cref="Guid"/> to <see cref="DistributedLockId"/>.
        /// </summary>
        /// <param name="id">
        /// The <see cref="Guid"/>.
        /// </param>
        /// <returns>
        /// The <see cref="DistributedLockId"/>.
        /// </returns>
        public static implicit operator DistributedLockId(Guid id) => new DistributedLockId(id);

        /// <summary>
        /// The equality operator.
        /// </summary>
        /// <param name="lockId">
        /// The <see cref="DistributedLockId"/>.
        /// </param>
        /// <param name="otherLockId">
        /// The <see cref="DistributedLockId"/>.
        /// </param>
        /// <returns>
        /// True if <paramref name="lockId"/> and <paramref name="otherLockId"/> are equal, false otherwise.
        /// </returns>
        public static bool operator ==(DistributedLockId lockId, DistributedLockId otherLockId) =>
            Equals(lockId, otherLockId);

        /// <summary>
        /// The inequality operator.
        /// </summary>
        /// <param name="lockId">
        /// The <see cref="DistributedLockId"/>.
        /// </param>
        /// <param name="otherLockId">
        /// The <see cref="DistributedLockId"/>.
        /// </param>
        /// <returns>
        /// True if <paramref name="lockId"/> and <paramref name="otherLockId"/> are not equal, false otherwise.
        /// </returns>
        public static bool operator !=(DistributedLockId lockId, DistributedLockId otherLockId) =>
            !(lockId == otherLockId);

        #endregion

        /// <inheritdoc />
        public override bool Equals(object obj) =>
            obj is DistributedLockId other &&
            other.GetHashCode() == GetHashCode();

        /// <inheritdoc />
        public override int GetHashCode() =>
            Value.GetHashCode();

        /// <inheritdoc />
        public override string ToString() => 
            Value.ToString();
    }
}