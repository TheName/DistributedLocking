using System;
using DistributedLocking.Abstractions.Records;

namespace DistributedLocking.Abstractions
{
    public interface IDistributedLock : IAsyncDisposable
    {
        DistributedLockId Id { get; }
        
        DistributedLockIdentifier Identifier { get; }
    }
}