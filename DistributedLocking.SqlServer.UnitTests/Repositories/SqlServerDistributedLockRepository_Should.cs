using System;
using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.Abstractions.Records;
using DistributedLocking.SqlServer.Abstractions.Configuration;
using DistributedLocking.SqlServer.Abstractions.Helpers;
using DistributedLocking.SqlServer.Repositories;
using DistributedLocking.SqlServer.UnitTests.Extensions;
using Moq;
using TestHelpers.Attributes;
using Xunit;

namespace DistributedLocking.SqlServer.UnitTests.Repositories
{
    public class SqlServerDistributedLockRepository_Should
    {
        private Mock<ISqlDistributedLocksTable> SqlDistributedLocksTableMock { get; } = new();
        
        private Mock<ISqlServerDistributedLockConfiguration> SqlServerDistributedLockConfigurationMock { get; } = new();

        private SqlServerDistributedLockRepository SqlServerDistributedLockRepository =>
            new(
                SqlDistributedLocksTableMock.Object,
                SqlServerDistributedLockConfigurationMock.Object);
        
        [Fact]
        public void Throw_When_Creating_And_SqlDistributedLocksTableIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new SqlServerDistributedLockRepository(null, SqlServerDistributedLockConfigurationMock.Object));
        }
        
        [Fact]
        public void Throw_When_Creating_And_SqlServerDistributedLockConfigurationMockIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new SqlServerDistributedLockRepository(SqlDistributedLocksTableMock.Object, null));
        }
        
        [Fact]
        public void NotThrow_When_Creating_And_AllParametersAreNotNull()
        {
            _ = new SqlServerDistributedLockRepository(
                SqlDistributedLocksTableMock.Object,
                SqlServerDistributedLockConfigurationMock.Object);
        }

        [Theory]
        [AutoMoqData]
        public async Task ReturnFalseAndNullAcquiredLockId_When_TryingToAcquireLock_And_SqlDistributedLocksTableReturnsFalse(
            DistributedLockIdentifier lockIdentifier,
            DistributedLockTimeToLive lockTimeToLive,
            string schemaName,
            string tableName)
        {
            SqlServerDistributedLockConfigurationMock
                .SetupSchemaName(schemaName)
                .SetupTableName(tableName);

            SqlDistributedLocksTableMock
                .Setup(table => table.TryInsertAsync(
                    schemaName,
                    tableName,
                    lockIdentifier.Value,
                    It.Is<Guid>(guid => guid != Guid.Empty),
                    lockTimeToLive.Value,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var (success, acquiredLockId) = await SqlServerDistributedLockRepository.TryAcquireAsync(   
                lockIdentifier,
                lockTimeToLive,
                CancellationToken.None);
            
            Assert.False(success);
            Assert.Null(acquiredLockId);
        }

        [Theory]
        [AutoMoqData]
        public async Task ReturnTrueAndAcquiredLockId_When_TryingToAcquireLock_And_SqlDistributedLocksTableReturnsFalse(
            DistributedLockIdentifier lockIdentifier,
            DistributedLockTimeToLive lockTimeToLive,
            string schemaName,
            string tableName)
        {
            SqlServerDistributedLockConfigurationMock
                .SetupSchemaName(schemaName)
                .SetupTableName(tableName);

            var lockIdPassedToTable = Guid.Empty;

            SqlDistributedLocksTableMock
                .Setup(table => table.TryInsertAsync(
                    schemaName,
                    tableName,
                    lockIdentifier.Value,
                    It.Is<Guid>(guid => guid != Guid.Empty),
                    lockTimeToLive.Value,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .Callback<string, string, Guid, Guid, TimeSpan, CancellationToken>((_, _, _, lockIdValue, _, _) =>
                    lockIdPassedToTable = lockIdValue);

            var (success, acquiredLockId) = await SqlServerDistributedLockRepository.TryAcquireAsync(   
                lockIdentifier,
                lockTimeToLive,
                CancellationToken.None);
            
            Assert.True(success);
            Assert.NotEqual(Guid.Empty, acquiredLockId.Value);
            Assert.Equal(lockIdPassedToTable, acquiredLockId.Value);
        }

        [Theory]
        [AutoMoqWithInlineData(true)]
        [AutoMoqWithInlineData(false)]
        public async Task ReturnResultFromTable_When_TryingToReleaseLock(
            bool success,
            DistributedLockIdentifier lockIdentifier,
            DistributedLockId lockId,
            string schemaName,
            string tableName)
        {
            SqlServerDistributedLockConfigurationMock
                .SetupSchemaName(schemaName)
                .SetupTableName(tableName);

            SqlDistributedLocksTableMock
                .Setup(table => table.TryDeleteAsync(
                    schemaName,
                    tableName,
                    lockIdentifier.Value,
                    lockId.Value,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(success);

            var result = await SqlServerDistributedLockRepository.TryReleaseAsync(lockIdentifier, lockId, CancellationToken.None);
            
            Assert.Equal(success, result);
        }

        [Theory]
        [AutoMoqWithInlineData(true)]
        [AutoMoqWithInlineData(false)]
        public async Task ReturnResultFromTable_When_TryingToExtendLock(
            bool success,
            DistributedLockIdentifier lockIdentifier,
            DistributedLockId lockId,
            DistributedLockTimeToLive additionalTimeToLive,
            string schemaName,
            string tableName)
        {
            SqlServerDistributedLockConfigurationMock
                .SetupSchemaName(schemaName)
                .SetupTableName(tableName);

            SqlDistributedLocksTableMock
                .Setup(table => table.TryUpdateAsync(
                    schemaName,
                    tableName,
                    lockIdentifier.Value,
                    lockId.Value,
                    additionalTimeToLive.Value,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(success);

            var result = await SqlServerDistributedLockRepository.TryExtendAsync(
                lockIdentifier,
                lockId,
                additionalTimeToLive,
                CancellationToken.None);
            
            Assert.Equal(success, result);
        }
    }
}