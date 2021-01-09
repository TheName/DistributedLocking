﻿using System;
using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.Abstractions.Factories;
using DistributedLocking.Abstractions.Managers;

namespace DistributedLocking.Managers
{
    public class DistributedLockRepositoryManagerProxy : IDistributedLockRepositoryManager
    {
        private readonly IDistributedLockRepositoryManagerFactory _repositoryManagerFactory;

        private IDistributedLockRepositoryManager RepositoryManager => _repositoryManagerFactory.Create();

        public DistributedLockRepositoryManagerProxy(IDistributedLockRepositoryManagerFactory repositoryManagerFactory)
        {
            _repositoryManagerFactory = repositoryManagerFactory ?? throw new ArgumentNullException(nameof(repositoryManagerFactory));
        }

        public async Task<bool> RepositoryExistsAsync(CancellationToken cancellationToken) =>
            await RepositoryManager.RepositoryExistsAsync(cancellationToken).ConfigureAwait(false);

        public async Task CreateIfNotExistsAsync(CancellationToken cancellationToken) =>
            await RepositoryManager.CreateIfNotExistsAsync(cancellationToken).ConfigureAwait(false);
    }
}