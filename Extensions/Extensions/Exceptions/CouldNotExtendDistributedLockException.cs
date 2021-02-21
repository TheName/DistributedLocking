using System;
using DistributedLocking.Abstractions;

// ReSharper disable once CheckNamespace
namespace DistributedLocking.Exceptions
{
    /// <summary>
    /// The <see cref="CouldNotExtendDistributedLockException"/>.
    /// <remarks>
    /// Thrown when extending a distributed lock with provided <see cref="DistributedLockResourceId"/> and <see cref="DistributedLockId"/> fails.
    /// </remarks>
    /// </summary>
    public class CouldNotExtendDistributedLockException : Exception
    {
        /// <summary>
        /// The <see cref="DistributedLockResourceId"/>.
        /// </summary>
        public DistributedLockResourceId ResourceId { get; }
        
        /// <summary>
        /// The <see cref="DistributedLockId"/>.
        /// </summary>
        public DistributedLockId Id { get; }
        
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
        /// <param name="id">
        /// The <see cref="DistributedLockId"/>.
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
        public CouldNotExtendDistributedLockException(
            DistributedLockResourceId resourceId,
            DistributedLockId id,
            DistributedLockTimeToLive timeToLive,
            Exception innerException = null)
            : base(CreateExceptionMessage(resourceId, id, timeToLive), innerException)
        {
            ResourceId = resourceId ?? throw new ArgumentNullException(nameof(resourceId));
            Id = id ?? throw new ArgumentNullException(nameof(id));
            TimeToLive = timeToLive ?? throw new ArgumentNullException(nameof(timeToLive));
        }

        private static string CreateExceptionMessage(
            DistributedLockResourceId resourceId,
            DistributedLockId id,
            DistributedLockTimeToLive timeToLive) =>
            $"Could not extend lock with resource id {resourceId} and lock id {id} with TTL {timeToLive}";
    }
}