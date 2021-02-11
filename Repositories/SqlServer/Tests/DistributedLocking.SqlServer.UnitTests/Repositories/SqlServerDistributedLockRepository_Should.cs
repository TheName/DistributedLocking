using System;
using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.Abstractions;
using DistributedLocking.Repositories.SqlServer;
using DistributedLocking.Repositories.SqlServer.Abstractions.Helpers;
using DistributedLocking.SqlServer.UnitTests.Extensions;
using Moq;
using TestHelpers.Attributes;
using Xunit;

namespace DistributedLocking.SqlServer.UnitTests.Repositories
{
    public class SqlServerDistributedLockRepository_Should
    {
        private Mock<ISqlClient> SqlClientMock { get; } = new();

        private SqlServerDistributedLocksRepository SqlServerDistributedLockRepository => new(SqlClientMock.Object);
        
        [Fact]
        public void Throw_When_Creating_And_SqlClientIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new SqlServerDistributedLocksRepository(null));
        }
        
        [Fact]
        public void NotThrow_When_Creating_And_AllParametersAreNotNull()
        {
            _ = new SqlServerDistributedLocksRepository(SqlClientMock.Object);
        }

        [Theory]
        [AutoMoqData]
        public async Task ReturnFalse_When_TryingToInsert_And_SqlClientReturnsZeroModifiedRows(
            DistributedLockIdentifier identifier,
            DistributedLockId lockId,
            DistributedLockTimeToLive timeToLive)
        {
            SqlClientMock.SetupNonQueryResult(0);
            
            var success = await SqlServerDistributedLockRepository.TryInsert(   
                identifier,
                lockId,
                timeToLive,
                CancellationToken.None);
            
            Assert.False(success);
        }

        [Theory]
        [AutoMoqData]
        public async Task ReturnTrue_When_TryingToInsert_And_SqlClientReturnsOneModifiedRow(
            DistributedLockIdentifier identifier,
            DistributedLockId lockId,
            DistributedLockTimeToLive timeToLive)
        {
            SqlClientMock.SetupNonQueryResult(1);

            var success = await SqlServerDistributedLockRepository.TryInsert(
                identifier,
                lockId,
                timeToLive,
                CancellationToken.None);
            
            Assert.True(success);
        }

        [Theory]
        [AutoMoqWithInlineData(-1)]
        [AutoMoqWithInlineData(2)]
        public async Task Throw_When_TryingToInsert_And_SqlClientReturnsOtherThanOneOrZeroModifiedRows(
            int numberOfModifiedRows,
            DistributedLockIdentifier identifier,
            DistributedLockId lockId,
            DistributedLockTimeToLive timeToLive)
        {
            SqlClientMock.SetupNonQueryResult(numberOfModifiedRows);

            await Assert.ThrowsAsync<InvalidOperationException>(() => SqlServerDistributedLockRepository.TryInsert(
                identifier,
                lockId,
                timeToLive,
                CancellationToken.None));
        }

        [Theory]
        [AutoMoqData]
        public async Task ReturnFalse_When_TryingToUpdateTimeToLive_And_SqlClientReturnsZeroModifiedRows(
            DistributedLockIdentifier identifier,
            DistributedLockId lockId,
            DistributedLockTimeToLive timeToLive)
        {
            SqlClientMock.SetupNonQueryResult(0);
            
            var success = await SqlServerDistributedLockRepository.TryUpdateTimeToLiveAsync(   
                identifier,
                lockId,
                timeToLive,
                CancellationToken.None);
            
            Assert.False(success);
        }

        [Theory]
        [AutoMoqData]
        public async Task ReturnTrue_When_TryingToUpdateTimeToLive_And_SqlClientReturnsOneModifiedRow(
            DistributedLockIdentifier identifier,
            DistributedLockId lockId,
            DistributedLockTimeToLive timeToLive)
        {
            SqlClientMock.SetupNonQueryResult(1);

            var success = await SqlServerDistributedLockRepository.TryUpdateTimeToLiveAsync(
                identifier,
                lockId,
                timeToLive,
                CancellationToken.None);
            
            Assert.True(success);
        }

        [Theory]
        [AutoMoqWithInlineData(-1)]
        [AutoMoqWithInlineData(2)]
        public async Task Throw_When_TryingToUpdateTimeToLive_And_SqlClientReturnsOtherThanOneOrZeroModifiedRows(
            int numberOfModifiedRows,
            DistributedLockIdentifier identifier,
            DistributedLockId lockId,
            DistributedLockTimeToLive timeToLive)
        {
            SqlClientMock.SetupNonQueryResult(numberOfModifiedRows);

            await Assert.ThrowsAsync<InvalidOperationException>(() => SqlServerDistributedLockRepository.TryUpdateTimeToLiveAsync(
                identifier,
                lockId,
                timeToLive,
                CancellationToken.None));
        }

        [Theory]
        [AutoMoqData]
        public async Task ReturnFalse_When_TryingToDelete_And_SqlClientReturnsZeroModifiedRows(
            DistributedLockIdentifier identifier,
            DistributedLockId lockId)
        {
            SqlClientMock.SetupNonQueryResult(0);
            
            var success = await SqlServerDistributedLockRepository.TryDelete(   
                identifier,
                lockId,
                CancellationToken.None);
            
            Assert.False(success);
        }

        [Theory]
        [AutoMoqData]
        public async Task ReturnTrue_When_TryingToDelete_And_SqlClientReturnsOneModifiedRow(
            DistributedLockIdentifier identifier,
            DistributedLockId lockId)
        {
            SqlClientMock.SetupNonQueryResult(1);

            var success = await SqlServerDistributedLockRepository.TryDelete(
                identifier,
                lockId,
                CancellationToken.None);
            
            Assert.True(success);
        }

        [Theory]
        [AutoMoqWithInlineData(-1)]
        [AutoMoqWithInlineData(2)]
        public async Task Throw_When_TryingToDelete_And_SqlClientReturnsOtherThanOneOrZeroModifiedRows(
            int numberOfModifiedRows,
            DistributedLockIdentifier identifier,
            DistributedLockId lockId)
        {
            SqlClientMock.SetupNonQueryResult(numberOfModifiedRows);

            await Assert.ThrowsAsync<InvalidOperationException>(() => SqlServerDistributedLockRepository.TryDelete(
                identifier,
                lockId,
                CancellationToken.None));
        }
    }
}