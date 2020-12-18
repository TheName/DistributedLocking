﻿using System;
using System.Threading;
using System.Threading.Tasks;
using TheName.DistributedLocking.Abstractions.Managers;
using TheName.DistributedLocking.SqlServer.Abstractions.Configuration;
using TheName.DistributedLocking.SqlServer.Abstractions.Helpers;

namespace TheName.DistributedLocking.SqlServer.Managers
{
    public class SqlServerDistributedLockRepositoryManager : IDistributedLockRepositoryManager
    {
        private readonly ISqlDistributedLocksTableManager _sqlDistributedLocksTableManager;
        private readonly ISqlServerDistributedLockConfiguration _configuration;

        internal SqlServerDistributedLockRepositoryManager(
            ISqlDistributedLocksTableManager sqlDistributedLocksTableManager,
            ISqlServerDistributedLockConfiguration configuration)
        {
            _sqlDistributedLocksTableManager = sqlDistributedLocksTableManager ?? throw new ArgumentNullException(nameof(sqlDistributedLocksTableManager));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }
        
        public async Task<bool> RepositoryExistsAsync(CancellationToken cancellationToken) =>
            await _sqlDistributedLocksTableManager
                .TableExistsAsync(_configuration.SchemaName, _configuration.TableName, cancellationToken)
                .ConfigureAwait(false);

        public async Task CreateIfNotExistsAsync(CancellationToken cancellationToken) =>
            await _sqlDistributedLocksTableManager.CreateTableIfNotExistsAsync(
                    _configuration.SchemaName,
                    _configuration.TableName,
                    _configuration.SqlApplicationLockTimeout,
                    cancellationToken)
                .ConfigureAwait(false);
    }
}