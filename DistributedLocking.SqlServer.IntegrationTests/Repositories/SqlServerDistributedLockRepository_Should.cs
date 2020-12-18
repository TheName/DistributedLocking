using System;
using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.SqlServer.IntegrationTests.CollectionDefinitions;
using DistributedLocking.SqlServer.IntegrationTests.Fixtures;
using TestHelpers.Attributes;
using TheName.DistributedLocking.Abstractions.Records;
using TheName.DistributedLocking.Abstractions.Repositories;
using Xunit;
using Xunit.Extensions.Ordering;

namespace DistributedLocking.SqlServer.IntegrationTests.Repositories
{
    [Collection(nameof(SqlDatabaseConnectionDefinition)), Order(1)]
    public class SqlServerDistributedLockRepository_Should
    {
        private readonly SqlDatabaseFixture _sqlDatabaseFixture;

        private IDistributedLockRepository DistributedLockRepository =>
            _sqlDatabaseFixture.GetService<IDistributedLockRepository>();

        public SqlServerDistributedLockRepository_Should(SqlDatabaseFixture sqlDatabaseFixture)
        {
            _sqlDatabaseFixture = sqlDatabaseFixture ?? throw new ArgumentNullException(nameof(sqlDatabaseFixture));
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
    }
}