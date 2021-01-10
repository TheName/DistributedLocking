using System;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedLocking.Abstractions
{
    public interface IDistributedLock : IAsyncDisposable
    {
        DistributedLockId Id { get; }
        
        DistributedLockIdentifier Identifier { get; }

        Task<bool> TryExtendAsync(DistributedLockTimeToLive timeToLive, CancellationToken cancellationToken);
    }
}