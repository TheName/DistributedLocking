using DistributedLocking.Abstractions.Factories;
using DistributedLocking.SqlServer.Factories;
using DistributedLocking.SqlServer.Managers;
using DistributedLocking.SqlServer.Repositories;
using TestHelpers.Attributes;
using Xunit;

namespace DistributedLocking.SqlServer.UnitTests.Factories
{
    public class SqlServerDistributedLockFactory_Should
    {
        [Theory]
        [AutoMoqData]
        public void Create_SqlServerDistributedLockRepository(SqlServerDistributedLockFactory sqlServerDistributedLockFactory)
        {
            var repositoryFactory = sqlServerDistributedLockFactory as IDistributedLockRepositoryFactory;
            var repository = repositoryFactory.Create();
            
            Assert.IsType<SqlServerDistributedLockRepository>(repository);
        }
        
        [Theory]
        [AutoMoqData]
        public void Create_SqlServerDistributedLockRepositoryManager(SqlServerDistributedLockFactory sqlServerDistributedLockFactory)
        {
            var repositoryFactory = sqlServerDistributedLockFactory as IDistributedLockRepositoryManagerFactory;
            var repository = repositoryFactory.Create();
            
            Assert.IsType<SqlServerDistributedLockRepositoryManager>(repository);
        }
    }
}