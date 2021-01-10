using System;
using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.Abstractions;
using DistributedLocking.Abstractions.Records;
using DistributedLocking.Abstractions.Repositories;

namespace DistributedLocking.Extensions.Abstractions.Repositories
{
    public static class DistributedLockRepositoryExtensions
    {
        public static async Task<(bool Success, IDistributedLock AcquiredLock)> TryAcquireLockAsync(
            this IDistributedLockRepository repository,
            DistributedLockIdentifier lockIdentifier,
            DistributedLockTimeToLive lockTimeToLive,
            CancellationToken cancellationToken)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            var (success, acquiredLockId) = await repository.TryAcquireAsync(   
                    lockIdentifier,
                    lockTimeToLive,
                    cancellationToken)
                .ConfigureAwait(false);

            if (!success)
            {
                return (false, null);
            }

            return (
                true,
                new DistributedLock(
                    acquiredLockId,
                    lockIdentifier,
                    repository));
        }

        public static async Task<bool> TryExtendAsync(
            this IDistributedLockRepository repository,
            IDistributedLock distributedLock,
            DistributedLockTimeToLive lockTimeToLive,
            CancellationToken cancellationToken)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            return await repository.TryExtendAsync(
                    distributedLock.LockIdentifier,
                    distributedLock.LockId,
                    lockTimeToLive,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        public static async Task<bool> TryReleaseAsync(
            this IDistributedLockRepository repository,
            IDistributedLock distributedLock,
            CancellationToken cancellationToken)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            return await repository.TryReleaseAsync(
                    distributedLock.LockIdentifier,
                    distributedLock.LockId,
                    cancellationToken)
                .ConfigureAwait(false);
        }
    }
}