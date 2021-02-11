using System;

namespace DistributedLocking.Abstractions
{
    /// <summary>
    /// The distributed lock identifier value object.
    /// <remarks>
    /// Identifies a "group"/"type" of distributed locks; there are never two active locks at the same time with the same identifier.
    /// </remarks>
    /// </summary>
    public sealed class DistributedLockIdentifier
    {
        /// <summary>
        /// The identifier value.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="value">
        /// The identifier.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown when provided <paramref name="value"/> is null or whitespace.
        /// </exception>
        public DistributedLockIdentifier(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"{nameof(DistributedLockIdentifier)} cannot be null or whitespace!", nameof(value));
            }
            
            Value = value;
        }

        #region Operators
        
        /// <summary>
        /// Implicit operator that converts the <see cref="DistributedLockIdentifier"/> to <see cref="string"/>.
        /// </summary>
        /// <param name="identifier">
        /// The <see cref="DistributedLockIdentifier"/>.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static implicit operator string(DistributedLockIdentifier identifier) => identifier.Value;
        
        /// <summary>
        /// Implicit operator that converts the <see cref="string"/> to <see cref="DistributedLockIdentifier"/>.
        /// </summary>
        /// <param name="identifier">
        /// The <see cref="string"/>.
        /// </param>
        /// <returns>
        /// The <see cref="DistributedLockIdentifier"/>.
        /// </returns>
        public static implicit operator DistributedLockIdentifier(string identifier) => 
            new DistributedLockIdentifier(identifier);

        /// <summary>
        /// Implicit operator that converts the <see cref="Guid"/> to <see cref="DistributedLockIdentifier"/>.
        /// </summary>
        /// <param name="identifier">
        /// The <see cref="Guid"/>.
        /// </param>
        /// <returns>
        /// The <see cref="DistributedLockIdentifier"/>.
        /// </returns>
        public static implicit operator DistributedLockIdentifier(Guid identifier) => identifier.ToString();

        /// <summary>
        /// The equality operator.
        /// </summary>
        /// <param name="lockIdentifier">
        /// The <see cref="DistributedLockIdentifier"/>.
        /// </param>
        /// <param name="otherLockIdentifier">
        /// The <see cref="DistributedLockIdentifier"/>.
        /// </param>
        /// <returns>
        /// True if <paramref name="lockIdentifier"/> and <paramref name="otherLockIdentifier"/> are equal, false otherwise.
        /// </returns>
        public static bool operator ==(DistributedLockIdentifier lockIdentifier, DistributedLockIdentifier otherLockIdentifier) =>
            Equals(lockIdentifier, otherLockIdentifier);

        /// <summary>
        /// The inequality operator.
        /// </summary>
        /// <param name="lockIdentifier">
        /// The <see cref="DistributedLockIdentifier"/>.
        /// </param>
        /// <param name="otherLockIdentifier">
        /// The <see cref="DistributedLockIdentifier"/>.
        /// </param>
        /// <returns>
        /// True if <paramref name="lockIdentifier"/> and <paramref name="otherLockIdentifier"/> are not equal, false otherwise.
        /// </returns>
        public static bool operator !=(DistributedLockIdentifier lockIdentifier, DistributedLockIdentifier otherLockIdentifier) =>
            !(lockIdentifier == otherLockIdentifier);

        #endregion

        /// <inheritdoc />
        public override bool Equals(object obj) =>
            obj is DistributedLockIdentifier other &&
            other.GetHashCode() == GetHashCode();

        /// <inheritdoc />
        public override int GetHashCode() => Value.GetHashCode();

        /// <inheritdoc />
        public override string ToString() => Value;
    }
}