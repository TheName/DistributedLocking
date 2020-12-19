using System;
using System.Threading;
using System.Threading.Tasks;
using TheName.DistributedLocking.Abstractions.Factories;
using TheName.DistributedLocking.Abstractions.Records;
using TheName.DistributedLocking.Abstractions.Repositories;

namespace TheName.DistributedLocking.Repositories
{
    public class DistributedLockRepositoryProxy : IDistributedLockRepository
    {
        private readonly IDistributedLockRepositoryFactory _repositoryFactory;

        private IDistributedLockRepository Repository => _repositoryFactory.Create();
        
        public DistributedLockRepositoryProxy(IDistributedLockRepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory ?? throw new ArgumentNullException(nameof(repositoryFactory));
        }

        public async Task<(bool Success, DistributedLockId AcquiredLockId)> TryAcquireAsync(
            DistributedLockIdentifier lockIdentifier,
            DistributedLockTimeToLive lockTimeToLive,
            CancellationToken cancellationToken) =>
            await Repository.TryAcquireAsync(
                    lockIdentifier,
                    lockTimeToLive,
                    cancellationToken)
                .ConfigureAwait(false);

        public async Task<bool> TryExtendAsync(
            DistributedLockIdentifier lockIdentifier,
            DistributedLockId lockId,
            DistributedLockTimeToLive lockTimeToLive,
            CancellationToken cancellationToken) =>
            await Repository.TryExtendAsync(
                    lockIdentifier,
                    lockId,
                    lockTimeToLive,
                    cancellationToken)
                .ConfigureAwait(false);

        public async Task<bool> TryReleaseAsync(DistributedLockId lockId, CancellationToken cancellationToken) =>
            await Repository.TryReleaseAsync(lockId, cancellationToken).ConfigureAwait(false);
    }
}