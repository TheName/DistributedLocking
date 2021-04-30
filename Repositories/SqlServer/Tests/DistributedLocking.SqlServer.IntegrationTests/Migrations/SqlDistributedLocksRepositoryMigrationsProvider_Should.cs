using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using DistributedLocking.Repositories.SqlServer.Abstractions.Configuration;
using DistributedLocking.Repositories.SqlServer.Migrations;
using DistributedLocking.SqlServer.IntegrationTests.CollectionDefinitions;
using DistributedLocking.SqlServer.IntegrationTests.Fixtures;
using Xunit;
using Xunit.Extensions.Ordering;

namespace DistributedLocking.SqlServer.IntegrationTests.Migrations
{
    [Collection(nameof(SqlDatabaseConnectionDefinition)), Order(0)]
    public class SqlDistributedLocksRepositoryMigrationsProvider_Should
    {
        private readonly SqlDatabaseFixture _sqlDatabaseFixture;

        private ISqlServerDistributedLockConfiguration SqlServerDistributedLockConfiguration =>
            _sqlDatabaseFixture.GetService<ISqlServerDistributedLockConfiguration>();

        public SqlDistributedLocksRepositoryMigrationsProvider_Should(SqlDatabaseFixture sqlDatabaseFixture)
        {
            _sqlDatabaseFixture = sqlDatabaseFixture ?? throw new ArgumentNullException(nameof(sqlDatabaseFixture));
        }

        [Fact, Order(0)]
        public async Task CreateTable_When_CreatingTableIfNotExistsMultipleTimes_And_TableDoesNotExist()
        {
            Assert.False(await TableExists());

            await Task.WhenAll(Enumerable.Repeat(0, 10).Select(i => ExecuteMigrationScripts()));
            
            Assert.True(await TableExists());
        }

        [Fact, Order(1)]
        public async Task NotThrow_When_CreatingTableIfNotExists_And_TableExists()
        {
            Assert.True(await TableExists());

            await ExecuteMigrationScripts();
            
            Assert.True(await TableExists());
        }

        private async Task<bool> TableExists()
        {
            const string commandText =
                "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'DistributedLocking' AND TABLE_NAME = 'DistributedLocks';";
            
            await using var connection = new SqlConnection(SqlServerDistributedLockConfiguration.ConnectionString);
            var command = new SqlCommand(commandText, connection);
            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return (int) result == 1;
        }

        private async Task ExecuteMigrationScripts()
        {
            var scriptsProvider = new SqlDistributedLocksRepositoryMigrationsProvider();
            var migrationScripts = await scriptsProvider.GetMigrationsAsync();
            foreach (var migrationScript in migrationScripts)
            {
                await ExecuteNonQuery(migrationScript.Content);
            }
        }

        private async Task ExecuteNonQuery(string commandText)
        {
            await using var connection = new SqlConnection(SqlServerDistributedLockConfiguration.ConnectionString);
            var command = new SqlCommand(commandText, connection);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }
    }
}