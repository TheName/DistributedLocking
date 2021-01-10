using System;
using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.Abstractions;
using DistributedLocking.Abstractions.Repositories;

namespace DistributedLocking.Extensions.Abstractions.Repositories
{
    public static class DistributedLockRepositoryExtensions
    {
        public static async Task<(bool Success, IDistributedLock AcquiredLock)> TryAcquireLockAsync(
            this IDistributedLockRepository repository,
            DistributedLockIdentifier identifier,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            var (success, acquiredLockId) = await repository.TryAcquireAsync(   
                    identifier,
                    timeToLive,
                    cancellationToken)
                .ConfigureAwait(false);

            if (!success)
            {
                return (false, null);
            }

            return (
                true,
                new DistributedLock(
                    identifier,
                    acquiredLockId,
                    repository));
        }

        public static async Task<bool> TryExtendAsync(
            this IDistributedLockRepository repository,
            IDistributedLock distributedLock,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            return await repository.TryExtendAsync(
                    distributedLock.Identifier,
                    distributedLock.Id,
                    timeToLive,
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
                    distributedLock.Identifier,
                    distributedLock.Id,
                    cancellationToken)
                .ConfigureAwait(false);
        }
    }
}