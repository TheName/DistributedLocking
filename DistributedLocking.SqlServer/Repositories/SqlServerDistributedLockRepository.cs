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

        internal SqlServerDistributedLockRepository(
            ISqlDistributedLocksTable sqlDistributedLocksTable,
            ISqlServerDistributedLockConfiguration configuration)
        {
            _sqlDistributedLocksTable = sqlDistributedLocksTable ?? throw new ArgumentNullException(nameof(sqlDistributedLocksTable));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }
        
        public async Task<(bool Success, DistributedLockId AcquiredLockId)> TryAcquireAsync(
            DistributedLockIdentifier lockIdentifier,
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

            return (result, result ? new DistributedLockId(lockId) : null);
        }

        public async Task<bool> TryReleaseAsync(DistributedLockId lockId, CancellationToken cancellationToken) =>
            await _sqlDistributedLocksTable
                .TryDeleteAsync(SchemaName, TableName, lockId.Value, cancellationToken)
                .ConfigureAwait(false);
    }
}