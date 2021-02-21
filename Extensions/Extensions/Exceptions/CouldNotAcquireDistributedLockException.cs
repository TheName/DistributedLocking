using System;
using DistributedLocking.Abstractions;

// ReSharper disable once CheckNamespace
namespace DistributedLocking.Exceptions
{
    /// <summary>
    /// The <see cref="CouldNotAcquireDistributedLockException"/>.
    /// <remarks>
    /// Thrown when acquiring of a distributed lock with provided <see cref="DistributedLockResourceId"/> fails.
    /// </remarks>
    /// </summary>
    public class CouldNotAcquireDistributedLockException : Exception
    {
        /// <summary>
        /// The <see cref="DistributedLockResourceId"/>.
        /// </summary>
        public DistributedLockResourceId ResourceId { get; }

        /// <summary>
        /// The <see cref="DistributedLockTimeToLive"/>.
        /// </summary>
        public DistributedLockTimeToLive TimeToLive { get; }
        
        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="resourceId">
        /// The <see cref="DistributedLockResourceId"/>.
        /// </param>
        /// <param name="timeToLive">
        /// The <see cref="DistributedLockTimeToLive"/>.
        /// </param>
        /// <param name="innerException">
        /// The <see cref="Exception"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when provided <paramref name="resourceId"/> or <paramref name="timeToLive"/> is null.
        /// </exception>
        public CouldNotAcquireDistributedLockException(
            DistributedLockResourceId resourceId,
            DistributedLockTimeToLive timeToLive,
            Exception innerException = null)
            : base(CreateExceptionMessage(resourceId, timeToLive), innerException)
        {
            ResourceId = resourceId ?? throw new ArgumentNullException(nameof(resourceId));
            TimeToLive = timeToLive ?? throw new ArgumentNullException(nameof(timeToLive));
        }

        private static string CreateExceptionMessage(
            DistributedLockResourceId resourceId,
            DistributedLockTimeToLive timeToLive) =>
            $"Could not acquire lock with resource id {resourceId} and time to live {timeToLive}.";
    }
}