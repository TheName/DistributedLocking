using System;

namespace DistributedLocking.Abstractions
{
    /// <summary>
    /// The distributed lock time to live (TTL) defining a <see cref="TimeSpan"/> of how long the lock should be active.
    /// </summary>
    public sealed class DistributedLockTimeToLive
    {
        private TimeSpan Value { get; }

        private DistributedLockTimeToLive(TimeSpan value)
        {
            if (value <= TimeSpan.Zero)
            {
                throw new ArgumentException($"{nameof(DistributedLockTimeToLive)} cannot be lower than or equal to zero!", nameof(value));
            }
            
            Value = value;
        }

        #region Operators
        
        /// <summary>
        /// Implicit operator that converts the <see cref="DistributedLockTimeToLive"/> to <see cref="TimeSpan"/>.
        /// </summary>
        /// <param name="timeToLive">
        /// The <see cref="DistributedLockTimeToLive"/>.
        /// </param>
        /// <returns>
        /// The <see cref="TimeSpan"/>.
        /// </returns>
        public static implicit operator TimeSpan(DistributedLockTimeToLive timeToLive) => timeToLive.Value;
        
        /// <summary>
        /// Implicit operator that converts the <see cref="TimeSpan"/> to <see cref="DistributedLockTimeToLive"/>.
        /// </summary>
        /// <param name="timeToLive">
        /// The <see cref="TimeSpan"/>.
        /// </param>
        /// <returns>
        /// The <see cref="DistributedLockTimeToLive"/>.
        /// </returns>
        public static implicit operator DistributedLockTimeToLive(TimeSpan timeToLive) => 
            new DistributedLockTimeToLive(timeToLive);

        /// <summary>
        /// The equality operator.
        /// </summary>
        /// <param name="timeToLive">
        /// The <see cref="DistributedLockTimeToLive"/>.
        /// </param>
        /// <param name="otherTimeToLive">
        /// The <see cref="DistributedLockTimeToLive"/>.
        /// </param>
        /// <returns>
        /// True if <paramref name="timeToLive"/> and <paramref name="otherTimeToLive"/> are equal, false otherwise.
        /// </returns>
        public static bool operator ==(DistributedLockTimeToLive timeToLive, DistributedLockTimeToLive otherTimeToLive) =>
            Equals(timeToLive, otherTimeToLive);

        /// <summary>
        /// The inequality operator.
        /// </summary>
        /// <param name="timeToLive">
        /// The <see cref="DistributedLockTimeToLive"/>.
        /// </param>
        /// <param name="otherTimeToLive">
        /// The <see cref="DistributedLockTimeToLive"/>.
        /// </param>
        /// <returns>
        /// True if <paramref name="timeToLive"/> and <paramref name="otherTimeToLive"/> are not equal, false otherwise.
        /// </returns>
        public static bool operator !=(DistributedLockTimeToLive timeToLive, DistributedLockTimeToLive otherTimeToLive) =>
            !(timeToLive == otherTimeToLive);

        #endregion

        /// <inheritdoc />
        public override bool Equals(object obj) =>
            obj is DistributedLockTimeToLive other &&
            other.GetHashCode() == GetHashCode();

        /// <inheritdoc />
        public override int GetHashCode() => Value.GetHashCode();

        /// <inheritdoc />
        public override string ToString() => Value.ToString();
    }
}