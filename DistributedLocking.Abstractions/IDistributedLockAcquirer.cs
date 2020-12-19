﻿using System.Threading;
using System.Threading.Tasks;
using TheName.DistributedLocking.Abstractions.Records;

namespace TheName.DistributedLocking.Abstractions
{
    public interface IDistributedLockAcquirer
    {
        Task<IDistributedLock> AcquireAsync(
            DistributedLockIdentifier lockIdentifier,
            LockTimeout lockTimeout,
            CancellationToken cancellationToken);
    }
}