using System;
using DistributedLocking.Abstractions;

// ReSharper disable once CheckNamespace
namespace DistributedLocking.Exceptions
{
    /// <summary>
    /// The <see cref="CouldNotAcquireDistributedLockException"/>.
    /// <remarks>
    /// Thrown when acquiring of a distributed lock with provided <see cref="DistributedLockIdentifier"/> fails.
    /// </remarks>
    /// </summary>
    public class CouldNotAcquireDistributedLockException : Exception
    {
        /// <summary>
        /// The <see cref="DistributedLockIdentifier"/>.
        /// </summary>
        public DistributedLockIdentifier Identifier { get; }

        /// <summary>
        /// The <see cref="DistributedLockTimeToLive"/>.
        /// </summary>
        public DistributedLockTimeToLive TimeToLive { get; }
        
        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="identifier">
        /// The <see cref="DistributedLockIdentifier"/>.
        /// </param>
        /// <param name="timeToLive">
        /// The <see cref="DistributedLockTimeToLive"/>.
        /// </param>
        /// <param name="innerException">
        /// The <see cref="Exception"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when provided <paramref name="identifier"/> or <paramref name="timeToLive"/> is null.
        /// </exception>
        public CouldNotAcquireDistributedLockException(
            DistributedLockIdentifier identifier,
            DistributedLockTimeToLive timeToLive,
            Exception innerException = null)
            : base(CreateExceptionMessage(identifier, timeToLive), innerException)
        {
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
            TimeToLive = timeToLive ?? throw new ArgumentNullException(nameof(timeToLive));
        }

        private static string CreateExceptionMessage(
            DistributedLockIdentifier identifier,
            DistributedLockTimeToLive timeToLive) =>
            $"Could not acquire lock with identifier {identifier} and time to live {timeToLive}.";
    }
}