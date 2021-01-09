using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.Abstractions.Managers;
using DistributedLocking.SqlServer.Abstractions.Configuration;
using DistributedLocking.SqlServer.IntegrationTests.CollectionDefinitions;
using DistributedLocking.SqlServer.IntegrationTests.Fixtures;
using Xunit;
using Xunit.Extensions.Ordering;

namespace DistributedLocking.SqlServer.IntegrationTests.Managers
{
    [Collection(nameof(SqlDatabaseConnectionDefinition)), Order(0)]
    public class SqlServerDistributedLockRepositoryManager_Should
    {
        private readonly SqlDatabaseFixture _sqlDatabaseFixture;

        private IDistributedLockRepositoryManager DistributedLockRepositoryManager =>
            _sqlDatabaseFixture.GetService<IDistributedLockRepositoryManager>();

        private ISqlServerDistributedLockConfiguration SqlServerDistributedLockConfiguration =>
            _sqlDatabaseFixture.GetService<ISqlServerDistributedLockConfiguration>();

        public SqlServerDistributedLockRepositoryManager_Should(SqlDatabaseFixture sqlDatabaseFixture)
        {
            _sqlDatabaseFixture = sqlDatabaseFixture ?? throw new ArgumentNullException(nameof(sqlDatabaseFixture));
        }

        [Fact, Order(0)]
        public async Task ReturnFalse_When_CheckingIfTableExists_And_TableDoesNotExist()
        {
            Assert.False(await TableExists());
            
            var exists = await DistributedLockRepositoryManager.RepositoryExistsAsync(CancellationToken.None);
            
            Assert.False(exists);
        }

        [Fact, Order(1)]
        public async Task CreateTable_When_CreatingTableIfNotExists_And_TableDoesNotExist()
        {
            Assert.False(await TableExists());

            await DistributedLockRepositoryManager.CreateIfNotExistsAsync(CancellationToken.None);
            
            Assert.True(await TableExists());
        }

        [Fact, Order(2)]
        public async Task ReturnTrue_When_CheckingIfTableExists_And_TableExists()
        {
            Assert.True(await TableExists());

            var exists = await DistributedLockRepositoryManager.RepositoryExistsAsync(CancellationToken.None);
            
            Assert.True(exists);
        }

        [Fact, Order(3)]
        public async Task NotThrow_When_CreatingTableIfNotExists_And_TableExists()
        {
            Assert.True(await TableExists());

            await DistributedLockRepositoryManager.CreateIfNotExistsAsync(CancellationToken.None);
        }

        private async Task<bool> TableExists()
        {
            var commandText = $"SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{SqlServerDistributedLockConfiguration.SchemaName}' AND TABLE_NAME = '{SqlServerDistributedLockConfiguration.TableName}';";
            await using var connection = new SqlConnection(SqlServerDistributedLockConfiguration.ConnectionString);
            var command = new SqlCommand(commandText, connection);
            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return result != null;
        }
    }
}