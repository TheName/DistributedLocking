using System;
using DistributedLocking.Abstractions;

// ReSharper disable once CheckNamespace
namespace DistributedLocking.Exceptions
{
    /// <summary>
    /// The <see cref="CouldNotReleaseDistributedLockException"/>.
    /// <remarks>
    /// Thrown when releasing of a distributed lock with provided <see cref="DistributedLockIdentifier"/> and <see cref="DistributedLockId"/> fails.
    /// </remarks>
    /// </summary>
    public class CouldNotReleaseDistributedLockException : Exception
    {
        /// <summary>
        /// The <see cref="DistributedLockIdentifier"/>.
        /// </summary>
        public DistributedLockIdentifier Identifier { get; }

        /// <summary>
        /// The <see cref="DistributedLockId"/>.
        /// </summary>
        public DistributedLockId Id { get; }

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="identifier">
        /// The <see cref="DistributedLockIdentifier"/>.
        /// </param>
        /// <param name="id">
        /// The <see cref="DistributedLockId"/>.
        /// </param>
        /// <param name="innerException">
        /// The <see cref="Exception"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when provided <paramref name="identifier"/> is null.
        /// </exception>
        public CouldNotReleaseDistributedLockException(
            DistributedLockIdentifier identifier,
            DistributedLockId id,
            Exception innerException = null)
            : base(CreateExceptionMessage(identifier, id), innerException)
        {
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
            Id = id ?? throw new ArgumentNullException(nameof(id));
        }

        private static string CreateExceptionMessage(
            DistributedLockIdentifier identifier,
            DistributedLockId id) =>
            $"Could not release lock with identifier {identifier} and  ID {id}";
    }
}