using DistributedLocking.Abstractions.Repositories;
using DistributedLocking.Cassandra.IntegrationTests.CollectionDefinitions;
using DistributedLocking.Cassandra.IntegrationTests.Fixtures;
using TestHelpers;
using Xunit;
using Xunit.Extensions.Ordering;

namespace DistributedLocking.Cassandra.IntegrationTests.Repositories
{
    [Collection(nameof(CassandraDatabaseConnectionDefinition)), Order(1)]
    public class CassandraDistributedLockRepository_Should : BaseRepositoryIntegrationTest
    {
        private readonly CassandraDatabaseFixture _cassandraDatabaseFixture;

        public CassandraDistributedLockRepository_Should(CassandraDatabaseFixture cassandraDatabaseFixture)
        {
            _cassandraDatabaseFixture = cassandraDatabaseFixture;
        }

        protected override IDistributedLocksRepository DistributedLockRepository =>
            _cassandraDatabaseFixture.GetService<IDistributedLocksRepository>();
    }
}