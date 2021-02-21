using System;

namespace DistributedLocking.Abstractions
{
    /// <summary>
    /// The distributed lock resource id value object.
    /// <remarks>
    /// Identifies a resource; there are never two active locks at the same time with the same resource id.
    /// </remarks>
    /// </summary>
    public sealed class DistributedLockResourceId
    {
        /// <summary>
        /// The resource id value.
        /// </summary>
        public string Value { get; }

        private DistributedLockResourceId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"{nameof(DistributedLockResourceId)} cannot be null or whitespace!", nameof(value));
            }
            
            Value = value;
        }

        #region Operators
        
        /// <summary>
        /// Implicit operator that converts the <see cref="DistributedLockResourceId"/> to <see cref="string"/>.
        /// </summary>
        /// <param name="resourceId">
        /// The <see cref="DistributedLockResourceId"/>.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static implicit operator string(DistributedLockResourceId resourceId) => resourceId.Value;
        
        /// <summary>
        /// Implicit operator that converts the <see cref="string"/> to <see cref="DistributedLockResourceId"/>.
        /// </summary>
        /// <param name="resourceId">
        /// The <see cref="string"/>.
        /// </param>
        /// <returns>
        /// The <see cref="DistributedLockResourceId"/>.
        /// </returns>
        public static implicit operator DistributedLockResourceId(string resourceId) => 
            new DistributedLockResourceId(resourceId);

        /// <summary>
        /// Implicit operator that converts the <see cref="Guid"/> to <see cref="DistributedLockResourceId"/>.
        /// </summary>
        /// <param name="resourceId">
        /// The <see cref="Guid"/>.
        /// </param>
        /// <returns>
        /// The <see cref="DistributedLockResourceId"/>.
        /// </returns>
        public static implicit operator DistributedLockResourceId(Guid resourceId) => resourceId.ToString();

        /// <summary>
        /// The equality operator.
        /// </summary>
        /// <param name="resourceId">
        /// The <see cref="DistributedLockResourceId"/>.
        /// </param>
        /// <param name="otherResourceId">
        /// The <see cref="DistributedLockResourceId"/>.
        /// </param>
        /// <returns>
        /// True if <paramref name="resourceId"/> and <paramref name="otherResourceId"/> are equal, false otherwise.
        /// </returns>
        public static bool operator ==(DistributedLockResourceId resourceId, DistributedLockResourceId otherResourceId) =>
            Equals(resourceId, otherResourceId);

        /// <summary>
        /// The inequality operator.
        /// </summary>
        /// <param name="resourceId">
        /// The <see cref="DistributedLockResourceId"/>.
        /// </param>
        /// <param name="otherResourceId">
        /// The <see cref="DistributedLockResourceId"/>.
        /// </param>
        /// <returns>
        /// True if <paramref name="resourceId"/> and <paramref name="otherResourceId"/> are not equal, false otherwise.
        /// </returns>
        public static bool operator !=(DistributedLockResourceId resourceId, DistributedLockResourceId otherResourceId) =>
            !(resourceId == otherResourceId);

        #endregion

        /// <inheritdoc />
        public override bool Equals(object obj) =>
            obj is DistributedLockResourceId other &&
            other.GetHashCode() == GetHashCode();

        /// <inheritdoc />
        public override int GetHashCode() => Value.GetHashCode();

        /// <inheritdoc />
        public override string ToString() => Value;
    }
}