using System;
using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.Abstractions.Repositories;

namespace DistributedLocking.Abstractions
{
    public class DistributedLock : IDistributedLock
    {
        public DistributedLockId Id { get; }
        
        public DistributedLockIdentifier Identifier { get; }

        private readonly IDistributedLockRepository _repository;

        public DistributedLock(
            DistributedLockIdentifier identifier,
            DistributedLockId id,
            IDistributedLockRepository repository)
        {
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
            Id = id ?? throw new ArgumentNullException(nameof(id));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }
        
        public async Task<bool> TryExtendAsync(DistributedLockTimeToLive timeToLive, CancellationToken cancellationToken) =>
            await _repository.TryUpdateTimeToLiveAsync(Identifier, Id, timeToLive, cancellationToken)
                .ConfigureAwait(false);

        public async ValueTask DisposeAsync()
        {
            await _repository.TryDeleteIfExistsAsync(
                    Identifier,
                    Id,
                    CancellationToken.None)
                .ConfigureAwait(false);
        }
    }
}