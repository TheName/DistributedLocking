using System;
using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.Abstractions.Records;
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
        
        public async Task<(bool Success, DistributedLockId AcquiredLockId)> TryAcquireAsync(
            DistributedLockIdentifier lockIdentifier,
            DistributedLockTimeToLive lockTimeToLive,
            CancellationToken cancellationToken)
        {
            var lockId = Guid.NewGuid();
            var result = await _sqlDistributedLocksTable.TryInsertAsync(
                    SchemaName,
                    TableName,
                    lockIdentifier.Value,
                    lockId,
                    lockTimeToLive.Value,
                    cancellationToken)
                .ConfigureAwait(false);

            return (result, result ? new DistributedLockId(lockId) : null);
        }

        public async Task<bool> TryExtendAsync(
            DistributedLockIdentifier lockIdentifier,
            DistributedLockId lockId,
            DistributedLockTimeToLive lockTimeToLive,
            CancellationToken cancellationToken) =>
            await _sqlDistributedLocksTable
                .TryUpdateAsync(SchemaName, TableName, lockIdentifier.Value, lockId.Value, lockTimeToLive.Value, cancellationToken)
                .ConfigureAwait(false);

        public async Task<bool> TryReleaseAsync(
            DistributedLockIdentifier lockIdentifier,
            DistributedLockId lockId,
            CancellationToken cancellationToken) =>
            await _sqlDistributedLocksTable
                .TryDeleteAsync(SchemaName, TableName, lockIdentifier.Value, lockId.Value, cancellationToken)
                .ConfigureAwait(false);
    }
}