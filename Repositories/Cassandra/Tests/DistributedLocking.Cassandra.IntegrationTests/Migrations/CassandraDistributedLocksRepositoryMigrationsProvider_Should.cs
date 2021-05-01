using System;
using System.Linq;
using System.Threading.Tasks;
using Cassandra;
using DistributedLocking.Cassandra.IntegrationTests.CollectionDefinitions;
using DistributedLocking.Cassandra.IntegrationTests.Fixtures;
using DistributedLocking.Repositories.Cassandra.Migrations;
using Xunit;
using Xunit.Extensions.Ordering;

namespace DistributedLocking.Cassandra.IntegrationTests.Migrations
{
    [Collection(nameof(CassandraDatabaseConnectionDefinition)), Order(0)]
    public class CassandraDistributedLocksRepositoryMigrationsProvider_Should
    {
        private readonly CassandraDatabaseFixture _cassandraDatabaseFixture;

        public CassandraDistributedLocksRepositoryMigrationsProvider_Should(CassandraDatabaseFixture cassandraDatabaseFixture)
        {
            _cassandraDatabaseFixture = cassandraDatabaseFixture;
        }

        [Fact]
        public async Task CreateTable_WithoutErrors()
        {
            Assert.False(await TableExists());

            await Task.WhenAll(Enumerable.Repeat(0, 10).Select(_ => ExecuteMigrationScripts()));
            
            Assert.True(await TableExists());
        }

        [Fact, Order(1)]
        public async Task NotThrow_When_CreatingTableIfNotExists_And_TableExists()
        {
            Assert.True(await TableExists());

            await ExecuteMigrationScripts();
            
            Assert.True(await TableExists());
        }

        private async Task ExecuteMigrationScripts()
        {
            var session = _cassandraDatabaseFixture.GetService<ISession>();
            var scriptsProvider = new CassandraDistributedLocksRepositoryMigrationsProvider(session.Keyspace);
            var migrationScripts = await scriptsProvider.GetMigrationsAsync();
            foreach (var migrationScript in migrationScripts)
            {
                await session.ExecuteAsync(new SimpleStatement(migrationScript.Content));
            }
        }

        private async Task<bool> TableExists()
        {
            var session = _cassandraDatabaseFixture.GetService<ISession>();

            var agreementAsync = await session.Cluster.Metadata.CheckSchemaAgreementAsync();
            if (!agreementAsync)
            {
                throw new Exception("Cluster is not in agreement!");
            }

            return session.Cluster.Metadata.GetKeyspace(_cassandraDatabaseFixture.Keyspace).GetTablesNames()
                .Contains("distributed_locks");
        }
    }
}