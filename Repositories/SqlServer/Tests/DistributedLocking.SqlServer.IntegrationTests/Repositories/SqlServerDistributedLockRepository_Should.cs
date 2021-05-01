using System;
using DistributedLocking.Abstractions.Repositories;
using DistributedLocking.SqlServer.IntegrationTests.CollectionDefinitions;
using DistributedLocking.SqlServer.IntegrationTests.Fixtures;
using TestHelpers;
using Xunit;
using Xunit.Extensions.Ordering;

namespace DistributedLocking.SqlServer.IntegrationTests.Repositories
{
    [Collection(nameof(SqlDatabaseConnectionDefinition)), Order(1)]
    public class SqlServerDistributedLockRepository_Should : BaseRepositoryIntegrationTest
    {
        private readonly SqlDatabaseFixture _sqlDatabaseFixture;

        protected override IDistributedLocksRepository DistributedLockRepository =>
            _sqlDatabaseFixture.GetService<IDistributedLocksRepository>();

        public SqlServerDistributedLockRepository_Should(SqlDatabaseFixture sqlDatabaseFixture)
        {
            _sqlDatabaseFixture = sqlDatabaseFixture ?? throw new ArgumentNullException(nameof(sqlDatabaseFixture));
        }
    }
}