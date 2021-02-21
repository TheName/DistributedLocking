using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using DistributedLocking.Abstractions;
using DistributedLocking.Abstractions.Repositories;
using Moq;
using TestHelpers.Attributes;
using Xunit;

namespace DistributedLocking.UnitTests
{
    public class DistributedLock_Should
    {
        [Theory]
        [AutoMoqData]
        public async Task Throw_When_TryingToAcquire_WithNullResourceId(
            DistributedLockTimeToLive timeToLive,
            IDistributedLocksRepository repository)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => DistributedLock.TryAcquireAsync(
                null,
                timeToLive,
                repository,
                CancellationToken.None));
        }
        
        [Theory]
        [AutoMoqData]
        public async Task Throw_When_TryingToAcquire_WithNullTimeToLive(
            DistributedLockResourceId resourceId,
            IDistributedLocksRepository repository)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => DistributedLock.TryAcquireAsync(
                resourceId,
                null,
                repository,
                CancellationToken.None));
        }
        
        [Theory]
        [AutoMoqData]
        public async Task Throw_When_TryingToAcquire_WithNullRepository(
            DistributedLockResourceId resourceId,
            DistributedLockTimeToLive timeToLive)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => DistributedLock.TryAcquireAsync(
                resourceId,
                timeToLive,
                null,
                CancellationToken.None));
        }
        
        [Theory]
        [AutoMoqData]
        public async Task NotThrow_When_TryingToAcquire_WithoutNulls(
            DistributedLockResourceId resourceId,
            DistributedLockTimeToLive timeToLive,
            IDistributedLocksRepository repository)
        {
            await DistributedLock.TryAcquireAsync(
                resourceId,
                timeToLive,
                repository,
                CancellationToken.None);
        }
        
        [Theory]
        [AutoMoqData]
        public async Task ReturnDistributedLock_When_TryingToAcquire_And_RepositorySucceeds(
            DistributedLockResourceId resourceId,
            DistributedLockTimeToLive timeToLive,
            Mock<IDistributedLocksRepository> repositoryMock)
        {
            repositoryMock
                .Setup(repository => repository.TryInsert(
                    resourceId,
                    It.IsAny<DistributedLockId>(),
                    timeToLive,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            
            var result = await DistributedLock.TryAcquireAsync(
                resourceId,
                timeToLive,
                repositoryMock.Object,
                CancellationToken.None);
            
            Assert.NotNull(result);
        }
        
        [Theory]
        [AutoMoqData]
        public async Task ReturnNull_When_TryingToAcquire_And_RepositoryFails(
            DistributedLockResourceId resourceId,
            DistributedLockTimeToLive timeToLive,
            Mock<IDistributedLocksRepository> repositoryMock)
        {
            repositoryMock
                .Setup(repository => repository.TryInsert(
                    resourceId,
                    It.IsAny<DistributedLockId>(),
                    timeToLive,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            
            var result = await DistributedLock.TryAcquireAsync(
                resourceId,
                timeToLive,
                repositoryMock.Object,
                CancellationToken.None);
            
            Assert.Null(result);
        }

        [Theory]
        [AutoMoqWithInlineData(true)]
        [AutoMoqWithInlineData(false)]
        public async Task TryToExtendLock(
            bool expectedResult,
            DistributedLockResourceId resourceId,
            DistributedLockTimeToLive timeToLive,
            [Frozen] Mock<IDistributedLocksRepository> distributedLocksRepositoryMock,
            CancellationToken cancellationToken)
        {
            var distributedLock = await AcquireDistributedLockAsync(
                resourceId,
                timeToLive,
                distributedLocksRepositoryMock);
            
            distributedLocksRepositoryMock
                .Setup(repository => repository.TryUpdateTimeToLiveAsync(
                    distributedLock.ResourceId,
                    distributedLock.Id,
                    timeToLive,
                    cancellationToken))
                .ReturnsAsync(expectedResult);
            
            var result = await distributedLock.TryExtendAsync(timeToLive, cancellationToken);

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [AutoMoqData]
        public async Task TryToReleaseLock_When_Disposing(
            DistributedLockResourceId resourceId,
            DistributedLockTimeToLive timeToLive,
            [Frozen] Mock<IDistributedLocksRepository> distributedLocksRepositoryMock)
        {
            var distributedLock = await AcquireDistributedLockAsync(
                resourceId,
                timeToLive,
                distributedLocksRepositoryMock);
            
            await distributedLock.DisposeAsync();

            distributedLocksRepositoryMock
                .Verify(
                    repository => repository.TryDelete(
                        distributedLock.ResourceId,
                        distributedLock.Id,
                        CancellationToken.None),
                    Times.Once);
        }

        private static async Task<DistributedLock> AcquireDistributedLockAsync(
            DistributedLockResourceId resourceId,
            DistributedLockTimeToLive timeToLive,
            [Frozen] Mock<IDistributedLocksRepository> distributedLocksRepositoryMock)
        {
            distributedLocksRepositoryMock
                .Setup(repository => repository.TryInsert(
                    resourceId,
                    It.IsAny<DistributedLockId>(),
                    timeToLive,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            
            return await DistributedLock.TryAcquireAsync(
                resourceId,
                timeToLive,
                distributedLocksRepositoryMock.Object,
                CancellationToken.None);
        }
    }
}