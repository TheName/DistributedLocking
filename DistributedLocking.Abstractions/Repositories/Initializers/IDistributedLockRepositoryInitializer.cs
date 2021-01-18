﻿using System.Threading;
using System.Threading.Tasks;

namespace DistributedLocking.Abstractions.Repositories.Initializers
{
    public interface IDistributedLockRepositoryInitializer
    {
        Task InitializeAsync(CancellationToken cancellationToken);
    }
}