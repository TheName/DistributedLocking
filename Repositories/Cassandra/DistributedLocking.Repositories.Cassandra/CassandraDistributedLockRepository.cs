using System;
using System.Threading;
using System.Threading.Tasks;
using Cassandra;
using DistributedLocking.Abstractions;
using DistributedLocking.Abstractions.Repositories;
using DistributedLocking.Repositories.Cassandra.Extensions;

namespace DistributedLocking.Repositories.Cassandra
{
    internal class CassandraDistributedLockRepository : IDistributedLocksRepository
    {
        private readonly ISession _session;
        
        private Task<PreparedStatement> InsertStatement { get; }
        
        private Task<PreparedStatement> UpdateStatement { get; }
        
        private Task<PreparedStatement> DeleteStatement { get; }

        public CassandraDistributedLockRepository(ISession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));

            InsertStatement =
                _session.PrepareAsync(
                    "UPDATE distributed_locks USING TTL :ttl SET id = :id WHERE resource_id = :resource_id IF id = NULL;");

            UpdateStatement =
                _session.PrepareAsync(
                    "UPDATE distributed_locks USING TTL :ttl SET id = :id WHERE resource_id = :resource_id IF id = :id;");

            DeleteStatement =
                _session.PrepareAsync("DELETE FROM distributed_locks WHERE resource_id = :resource_id IF id = :id;");
        }
        
        public async Task<bool> TryInsert(
            DistributedLockResourceId resourceId,
            DistributedLockId id,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken)
        {
            var preparedInsertStatement = await InsertStatement.ConfigureAwait(false);
            TimeSpan ttl = timeToLive ?? throw new ArgumentNullException(nameof(timeToLive));
            Guid idGuid = id ?? throw new ArgumentNullException(nameof(id));

            var parameters = new
            {
                resource_id = resourceId?.ToString() ?? throw new ArgumentNullException(nameof(resourceId)),
                id = idGuid,
                ttl = (int) ttl.TotalSeconds
            };

            var boundStatement = preparedInsertStatement
                .Bind(parameters)
                .SetConsistencyLevel(ConsistencyLevel.Quorum)
                .SetSerialConsistencyLevel(ConsistencyLevel.Serial);

            var result = await _session.ExecuteAsync(boundStatement).ConfigureAwait(false);
            return result.IsApplied();
        }

        public async Task<bool> TryUpdateTimeToLiveAsync(
            DistributedLockResourceId resourceId,
            DistributedLockId id,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken)
        {
            var preparedUpdateStatement = await UpdateStatement.ConfigureAwait(false);
            TimeSpan ttl = timeToLive ?? throw new ArgumentNullException(nameof(timeToLive));
            Guid idGuid = id ?? throw new ArgumentNullException(nameof(id));

            var parameters = new
            {
                resource_id = resourceId?.ToString() ?? throw new ArgumentNullException(nameof(resourceId)),
                id = idGuid,
                ttl = (int) ttl.TotalSeconds
            };

            var boundStatement = preparedUpdateStatement
                .Bind(parameters)
                .SetConsistencyLevel(ConsistencyLevel.Quorum)
                .SetSerialConsistencyLevel(ConsistencyLevel.Serial);

            var result = await _session.ExecuteAsync(boundStatement).ConfigureAwait(false);
            return result.IsApplied();
        }

        public async Task<bool> TryDelete(
            DistributedLockResourceId resourceId,
            DistributedLockId id,
            CancellationToken cancellationToken)
        {
            var preparedDeleteStatement = await DeleteStatement.ConfigureAwait(false);
            Guid idGuid = id ?? throw new ArgumentNullException(nameof(id));

            var parameters = new
            {
                resource_id = resourceId?.ToString() ?? throw new ArgumentNullException(nameof(resourceId)),
                id = idGuid
            };

            var boundStatement = preparedDeleteStatement
                .Bind(parameters)
                .SetConsistencyLevel(ConsistencyLevel.Quorum)
                .SetSerialConsistencyLevel(ConsistencyLevel.Serial);

            var result = await _session.ExecuteAsync(boundStatement).ConfigureAwait(false);
            return result.IsApplied();
        }
    }
}