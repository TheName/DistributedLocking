﻿using System;
using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.Abstractions;
using DistributedLocking.Abstractions.Repositories;
using DistributedLocking.SqlServer.Abstractions.Configuration;
using DistributedLocking.SqlServer.Abstractions.Helpers;

namespace DistributedLocking.SqlServer.Repositories
{
    public class SqlServerDistributedLockRepository : IDistributedLockRepository
    {
        private readonly ISqlDistributedLocksTable _sqlDistributedLocksTable;
        private readonly ISqlServerDistributedLockConfiguration _configuration;

        private string SchemaName => _configuration.SchemaName;
        private string TableName => _configuration.TableName;

        internal SqlServerDistributedLockRepository(
            ISqlDistributedLocksTable sqlDistributedLocksTable,
            ISqlServerDistributedLockConfiguration configuration)
        {
            _sqlDistributedLocksTable = sqlDistributedLocksTable ?? throw new ArgumentNullException(nameof(sqlDistributedLocksTable));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<bool> TryInsertIfIdentifierNotExistsAsync(
            DistributedLockIdentifier identifier,
            DistributedLockId id,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken) =>
            await _sqlDistributedLocksTable.TryInsertAsync(
                    SchemaName,
                    TableName,
                    identifier.Value,
                    id,
                    timeToLive.Value,
                    cancellationToken)
                .ConfigureAwait(false);

        public async Task<bool> TryUpdateTimeToLiveAsync(
            DistributedLockIdentifier identifier,
            DistributedLockId id,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken) =>
            await _sqlDistributedLocksTable
                .TryUpdateAsync(SchemaName, TableName, identifier.Value, id.Value, timeToLive.Value, cancellationToken)
                .ConfigureAwait(false);

        public async Task<bool> TryDeleteIfExistsAsync(
            DistributedLockIdentifier identifier,
            DistributedLockId id,
            CancellationToken cancellationToken) =>
            await _sqlDistributedLocksTable
                .TryDeleteAsync(SchemaName, TableName, identifier.Value, id.Value, cancellationToken)
                .ConfigureAwait(false);
    }
}