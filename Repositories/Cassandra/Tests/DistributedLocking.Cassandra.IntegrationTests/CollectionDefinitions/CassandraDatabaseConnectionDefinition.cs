using DistributedLocking.Cassandra.IntegrationTests.Fixtures;
using Xunit;

namespace DistributedLocking.Cassandra.IntegrationTests.CollectionDefinitions
{
    [CollectionDefinition(nameof(CassandraDatabaseConnectionDefinition))]
    public class CassandraDatabaseConnectionDefinition : ICollectionFixture<CassandraDatabaseFixture>
    {
    }
}