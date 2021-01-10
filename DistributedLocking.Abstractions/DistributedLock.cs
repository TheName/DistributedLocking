using System;
using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.Abstractions.Records;
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

        public async ValueTask DisposeAsync()
        {
            await _repository.TryReleaseAsync(
                    Identifier,
                    Id,
                    CancellationToken.None)
                .ConfigureAwait(false);
        }
    }
}