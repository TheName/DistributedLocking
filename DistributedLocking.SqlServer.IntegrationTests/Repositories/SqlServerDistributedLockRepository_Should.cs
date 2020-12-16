using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.SqlServer.IntegrationTests.CollectionDefinitions;
using DistributedLocking.SqlServer.IntegrationTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using TestHelpers.Attributes;
using TheName.DistributedLocking.Abstractions.Records;
using TheName.DistributedLocking.Abstractions.Repositories;
using TheName.DistributedLocking.SqlServer.Abstractions.Configuration;
using Xunit;
using Xunit.Extensions.Ordering;

namespace DistributedLocking.SqlServer.IntegrationTests.Repositories
{
    [Collection(nameof(SqlDatabaseConnectionDefinition))]
    public class SqlServerDistributedLockRepository_Should
    {
        private readonly SqlDatabaseFixture _sqlDatabaseFixture;

        private IDistributedLockRepository DistributedLockRepository =>
            _sqlDatabaseFixture.ServiceProvider.GetRequiredService<IDistributedLockRepository>();

        private ISqlServerDistributedLockConfiguration SqlServerDistributedLockConfiguration =>
            _sqlDatabaseFixture.ServiceProvider.GetRequiredService<ISqlServerDistributedLockConfiguration>();

        public SqlServerDistributedLockRepository_Should(SqlDatabaseFixture sqlDatabaseFixture)
        {
            _sqlDatabaseFixture = sqlDatabaseFixture ?? throw new ArgumentNullException(nameof(sqlDatabaseFixture));
        }

        [Fact, Order(0)]
        public async Task CreateTableIfNotExists_And_TellIfRepositoryExists()
        {
            var doesTableExist = await TableExists(); 
            Assert.False(doesTableExist);
            Assert.Equal(doesTableExist, await DistributedLockRepository.ExistsAsync(CancellationToken.None));

            await DistributedLockRepository.CreateIfNotExistsAsync(CancellationToken.None);
            
            doesTableExist = await TableExists(); 
            Assert.True(doesTableExist);
            Assert.Equal(doesTableExist, await DistributedLockRepository.ExistsAsync(CancellationToken.None));
        }

        [Theory]
        [AutoMoqData]
        public async Task AcquireLock(
            LockIdentifier lockIdentifier,
            LockTimeout lockTimeout)
        {
            var (success, acquiredLockId) = await DistributedLockRepository.TryAcquireAsync(lockIdentifier, lockTimeout, CancellationToken.None);
            
            Assert.True(success);
            Assert.NotEqual(Guid.Empty, acquiredLockId.Value);
        }

        [Theory]
        [AutoMoqData]
        public async Task AcquireLock_When_LockIdentifierIsAlreadyReleased(
            LockIdentifier lockIdentifier)
        {
            await DistributedLockRepository.CreateIfNotExistsAsync(CancellationToken.None);
            var lockTimeout = new LockTimeout(TimeSpan.FromMilliseconds(1));
            var (success, _) = await DistributedLockRepository.TryAcquireAsync(lockIdentifier, lockTimeout, CancellationToken.None);
            Assert.True(success);
            await Task.Delay(lockTimeout.Value + TimeSpan.FromMilliseconds(100));
            
            (success, _) = await DistributedLockRepository.TryAcquireAsync(lockIdentifier, lockTimeout, CancellationToken.None);
            
            Assert.True(success);
        }

        [Theory]
        [AutoMoqData]
        public async Task NotAcquireLock_When_LockIdentifierIsAlreadyTaken(
            LockIdentifier lockIdentifier,
            uint additionalTimeInMilliseconds)
        {
            await DistributedLockRepository.CreateIfNotExistsAsync(CancellationToken.None);
            var timeout = TimeSpan.FromSeconds(3) + TimeSpan.FromMilliseconds(additionalTimeInMilliseconds);
            var (success, _) = await DistributedLockRepository.TryAcquireAsync(lockIdentifier, new LockTimeout(timeout), CancellationToken.None);
            
            Assert.True(success);
            
            (success, _) = await DistributedLockRepository.TryAcquireAsync(lockIdentifier, new LockTimeout(timeout), CancellationToken.None);
            
            Assert.False(success);
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