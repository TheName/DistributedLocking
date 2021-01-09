using System;
using DistributedLocking.Abstractions.Records;

namespace DistributedLocking.Abstractions
{
    public interface IDistributedLock : IAsyncDisposable
    {
        DistributedLockId LockId { get; }
        
        DistributedLockIdentifier LockIdentifier { get; }
    }
}