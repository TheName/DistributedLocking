using System;
using DistributedLocking.Abstractions;

// ReSharper disable once CheckNamespace
namespace DistributedLocking.Exceptions
{
    /// <summary>
    /// The <see cref="CouldNotReleaseDistributedLockException"/>.
    /// <remarks>
    /// Thrown when releasing of a distributed lock with provided <see cref="DistributedLockResourceId"/> and <see cref="DistributedLockId"/> fails.
    /// </remarks>
    /// </summary>
    public class CouldNotReleaseDistributedLockException : Exception
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
        /// The constructor.
        /// </summary>
        /// <param name="resourceId">
        /// The <see cref="DistributedLockResourceId"/>.
        /// </param>
        /// <param name="id">
        /// The <see cref="DistributedLockId"/>.
        /// </param>
        /// <param name="innerException">
        /// The <see cref="Exception"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when provided <paramref name="resourceId"/> is null.
        /// </exception>
        public CouldNotReleaseDistributedLockException(
            DistributedLockResourceId resourceId,
            DistributedLockId id,
            Exception innerException = null)
            : base(CreateExceptionMessage(resourceId, id), innerException)
        {
            ResourceId = resourceId ?? throw new ArgumentNullException(nameof(resourceId));
            Id = id ?? throw new ArgumentNullException(nameof(id));
        }

        private static string CreateExceptionMessage(
            DistributedLockResourceId resourceId,
            DistributedLockId id) =>
            $"Could not release lock with resource id {resourceId} and lock id {id}";
    }
}