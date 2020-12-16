using System;
using System.Threading;
using System.Threading.Tasks;
using TheName.DistributedLocking.Abstractions.Records;
using TheName.DistributedLocking.Abstractions.Repositories;
using TheName.DistributedLocking.SqlServer.Abstractions.Configuration;
using TheName.DistributedLocking.SqlServer.Abstractions.Helpers;

namespace TheName.DistributedLocking.SqlServer.Repositories
{
    public class SqlServerDistributedLockRepository : IDistributedLockRepository
    {
        private readonly ISqlDistributedLocksTable _sqlDistributedLocksTable;
        private readonly ISqlServerDistributedLockConfiguration _configuration;

        private string SchemaName => _configuration.SchemaName;
        private string TableName => _configuration.TableName;

        private TimeSpan CreateTableIfNotExistsSqlApplicationLockTimeout => _configuration.SqlApplicationLockTimeout;

        internal SqlServerDistributedLockRepository(
            ISqlDistributedLocksTable sqlDistributedLocksTable,
            ISqlServerDistributedLockConfiguration configuration)
        {
            _sqlDistributedLocksTable = sqlDistributedLocksTable ?? throw new ArgumentNullException(nameof(sqlDistributedLocksTable));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }
        
        public async Task<(bool Success, LockId AcquiredLockId)> TryAcquireAsync(
            LockIdentifier lockIdentifier,
            LockTimeout lockTimeout,
            CancellationToken cancellationToken)
        {
            var lockId = Guid.NewGuid();
            var result = await _sqlDistributedLocksTable.TryInsertAsync(
                    SchemaName,
                    TableName,
                    lockIdentifier.Value,
                    lockId,
                    lockTimeout.Value,
                    cancellationToken)
                .ConfigureAwait(false);

            return (result, result ? new LockId(lockId) : null);
        }

        public async Task<bool> TryReleaseAsync(LockId lockId, CancellationToken cancellationToken) =>
            await _sqlDistributedLocksTable
                .TryDeleteAsync(SchemaName, TableName, lockId.Value, cancellationToken)
                .ConfigureAwait(false);

        public async Task<bool> ExistsAsync(CancellationToken cancellationToken) =>
            await _sqlDistributedLocksTable.TableExistsAsync(SchemaName, TableName, cancellationToken)
                .ConfigureAwait(false);

        public async Task CreateIfNotExistsAsync(CancellationToken cancellationToken) =>
            await _sqlDistributedLocksTable.CreateTableIfNotExistsAsync(
                    SchemaName,
                    TableName,
                    CreateTableIfNotExistsSqlApplicationLockTimeout,
                    cancellationToken)
                .ConfigureAwait(false);
    }
}