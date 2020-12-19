using System;
using TheName.DistributedLocking.Abstractions.Records;

namespace TheName.DistributedLocking.Abstractions
{
    public interface IDistributedLock : IAsyncDisposable
    {
        DistributedLockId LockId { get; }
        
        DistributedLockIdentifier LockIdentifier { get; }
    }
}