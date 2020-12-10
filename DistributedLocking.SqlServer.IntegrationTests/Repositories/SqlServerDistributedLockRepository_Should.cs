using System;
using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.SqlServer.IntegrationTests.CollectionDefinitions;
using DistributedLocking.SqlServer.IntegrationTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using TestHelpers.Attributes;
using TheName.DistributedLocking.Abstractions.Records;
using TheName.DistributedLocking.Abstractions.Repositories;
using Xunit;

namespace DistributedLocking.SqlServer.IntegrationTests.Repositories
{
    [Collection(nameof(SqlDatabaseConnectionDefinition))]
    public class SqlServerDistributedLockRepository_Should
    {
        private readonly SqlDatabaseFixture _sqlDatabaseFixture;

        public SqlServerDistributedLockRepository_Should(SqlDatabaseFixture sqlDatabaseFixture)
        {
            _sqlDatabaseFixture = sqlDatabaseFixture ?? throw new ArgumentNullException(nameof(sqlDatabaseFixture));
        }

        [Theory]
        [AutoMoqData]
        public async Task AcquireLockProperly(
            LockIdentifier lockIdentifier,
            LockTimeout lockTimeout)
        {
            var repository = _sqlDatabaseFixture.ServiceProvider.GetRequiredService<IDistributedLockRepository>();

            var (success, acquiredLockId) = await repository.TryAcquireAsync(lockIdentifier, lockTimeout, CancellationToken.None);
            
            Assert.True(success);
            Assert.NotEqual(Guid.Empty, acquiredLockId.Value);
        }
    }
}