using System;
using DistributedLocking.Abstractions;

// ReSharper disable once CheckNamespace
namespace DistributedLocking.Exceptions
{
    /// <summary>
    /// The <see cref="CouldNotExtendDistributedLockException"/>.
    /// <remarks>
    /// Thrown when extending a distributed lock with provided <see cref="DistributedLockIdentifier"/> and <see cref="DistributedLockId"/> fails.
    /// </remarks>
    /// </summary>
    public class CouldNotExtendDistributedLockException : Exception
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
        /// The <see cref="DistributedLockTimeToLive"/>.
        /// </summary>
        public DistributedLockTimeToLive TimeToLive { get; }

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="identifier">
        /// The <see cref="DistributedLockIdentifier"/>.
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
        /// Thrown when provided <paramref name="identifier"/> or <paramref name="timeToLive"/> is null.
        /// </exception>
        public CouldNotExtendDistributedLockException(
            DistributedLockIdentifier identifier,
            DistributedLockId id,
            DistributedLockTimeToLive timeToLive,
            Exception innerException = null)
            : base(CreateExceptionMessage(identifier, id, timeToLive), innerException)
        {
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
            Id = id ?? throw new ArgumentNullException(nameof(id));
            TimeToLive = timeToLive ?? throw new ArgumentNullException(nameof(timeToLive));
        }

        private static string CreateExceptionMessage(
            DistributedLockIdentifier identifier,
            DistributedLockId id,
            DistributedLockTimeToLive timeToLive) =>
            $"Could not extend lock with identifier {identifier} and  ID {id} with TTL {timeToLive}";
    }
}