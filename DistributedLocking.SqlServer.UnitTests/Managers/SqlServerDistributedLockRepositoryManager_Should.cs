using System;
using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.SqlServer.Abstractions.Configuration;
using DistributedLocking.SqlServer.Abstractions.Helpers;
using DistributedLocking.SqlServer.Managers;
using DistributedLocking.SqlServer.UnitTests.Extensions;
using Moq;
using TestHelpers.Attributes;
using Xunit;

namespace DistributedLocking.SqlServer.UnitTests.Managers
{
    public class SqlServerDistributedLockRepositoryManager_Should
    {
        private Mock<ISqlDistributedLocksTableManager> SqlDistributedLocksTableManagerMock { get; } = new();
        
        private Mock<ISqlServerDistributedLockConfiguration> SqlServerDistributedLockConfigurationMock { get; } = new();

        private SqlServerDistributedLockRepositoryManager SqlServerDistributedLockRepositoryManager =>
            new(
                SqlDistributedLocksTableManagerMock.Object,
                SqlServerDistributedLockConfigurationMock.Object);
        
        [Fact]
        public void Throw_When_Creating_And_SqlDistributedLocksTableManagerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new SqlServerDistributedLockRepositoryManager(null, SqlServerDistributedLockConfigurationMock.Object));
        }
        
        [Fact]
        public void Throw_When_Creating_And_SqlServerDistributedLockConfigurationMockIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new SqlServerDistributedLockRepositoryManager(SqlDistributedLocksTableManagerMock.Object, null));
        }
        
        [Fact]
        public void NotThrow_When_Creating_And_AllParametersAreNotNull()
        {
            _ = new SqlServerDistributedLockRepositoryManager(
                SqlDistributedLocksTableManagerMock.Object,
                SqlServerDistributedLockConfigurationMock.Object);
        }
        
        [Theory]
        [AutoMoqWithInlineData(true)]
        [AutoMoqWithInlineData(false)]
        public async Task ReturnResultFromTableManager_When_CallingRepositoryExistsAsync(
            bool tableExists,
            string schemaName,
            string tableName)
        {
            SqlServerDistributedLockConfigurationMock
                .SetupSchemaName(schemaName)
                .SetupTableName(tableName);

            SqlDistributedLocksTableManagerMock
                .Setup(manager => manager.TableExistsAsync(schemaName, tableName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tableExists);

            var result = await SqlServerDistributedLockRepositoryManager.RepositoryExistsAsync(CancellationToken.None);
            
            Assert.Equal(tableExists, result);
        }
        
        [Theory]
        [AutoMoqData]
        public async Task CallFromTableManagerWithCorrectParameters_When_CallingCreateIfNotExists(
            string schemaName,
            string tableName,
            TimeSpan sqlApplicationLockTimeout)
        {
            SqlServerDistributedLockConfigurationMock
                .SetupSchemaName(schemaName)
                .SetupTableName(tableName)
                .SetupSqlApplicationLockTimeout(sqlApplicationLockTimeout);

            await SqlServerDistributedLockRepositoryManager.CreateIfNotExistsAsync(CancellationToken.None);

            SqlDistributedLocksTableManagerMock
                .Verify(
                    manager => manager.CreateTableIfNotExistsAsync(
                        schemaName,
                        tableName,
                        sqlApplicationLockTimeout,
                        It.IsAny<CancellationToken>()),
                    Times.Once);
        }
    }
}