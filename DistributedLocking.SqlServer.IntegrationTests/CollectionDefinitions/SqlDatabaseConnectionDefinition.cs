using DistributedLocking.SqlServer.IntegrationTests.Fixtures;
using Xunit;

namespace DistributedLocking.SqlServer.IntegrationTests.CollectionDefinitions
{
    [CollectionDefinition(nameof(SqlDatabaseConnectionDefinition))]
    public class SqlDatabaseConnectionDefinition : ICollectionFixture<SqlDatabaseFixture>
    {
    }
}