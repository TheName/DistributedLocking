﻿using System.Threading;
using System.Threading.Tasks;
using TheName.DistributedLocking.Abstractions.Records;

namespace TheName.DistributedLocking.Abstractions
{
    public interface IDistributedLockAcquirer
    {
        Task<IDistributedLock> AcquireAsync(
            DistributedLockIdentifier lockIdentifier,
            DistributedLockTimeout lockTimeout,
            CancellationToken cancellationToken);
    }
}