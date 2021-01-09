using System;
using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.Abstractions.Initializers;
using DistributedLocking.Abstractions.Managers;

namespace DistributedLocking.Initializers
{
    public class DistributedLockRepositoryInitializer : IDistributedLockRepositoryInitializer
    {
        private readonly IDistributedLockRepositoryManager _manager;

        public DistributedLockRepositoryInitializer(IDistributedLockRepositoryManager manager)
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        }
        
        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            if (await _manager.RepositoryExistsAsync(cancellationToken).ConfigureAwait(false))
            {
                return;
            }

            await _manager.CreateIfNotExistsAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}