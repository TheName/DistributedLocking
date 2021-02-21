using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using DistributedLocking.Abstractions;
using DistributedLocking.Abstractions.Repositories;
using Moq;
using TestHelpers.Attributes;
using Xunit;

namespace DistributedLocking.UnitTests.Facades
{
    public class DistributedLockFacade_Should
    {
        [Fact]
        public void Throw_When_Creating_And_RepositoryIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new DistributedLockFacade(null));
        }
        
        [Theory]
        [AutoMoqData]
        public void NotThrow_When_Creating_And_AllParametersAreNotNull(IDistributedLocksRepository repository)
        {
            _ = new DistributedLockFacade(repository);
        }

        [Theory]
        [AutoMoqData]
        public async Task Throw_When_TryingToAcquire_And_ResourceIdIsNull(
            DistributedLockTimeToLive timeToLive,
            DistributedLockFacade facade)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => facade.TryAcquireAsync(
                null,
                timeToLive,
                CancellationToken.None));
        }

        [Theory]
        [AutoMoqData]
        public async Task Throw_When_TryingToAcquire_And_TimeToLiveIsNull(
            DistributedLockResourceId resourceId,
            DistributedLockFacade facade)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => facade.TryAcquireAsync(
                resourceId,
                null,
                CancellationToken.None));
        }

        [Theory]
        [AutoMoqData]
        public async Task ReturnNull_When_TryingToAcquire_And_RepositoryFails(
            DistributedLockResourceId resourceId,
            DistributedLockTimeToLive timeToLive,
            [Frozen] Mock<IDistributedLocksRepository> repositoryMock,
            DistributedLockFacade facade)
        {
            repositoryMock
                .Setup(lockRepository => lockRepository.TryInsert(
                    resourceId,
                    It.IsAny<DistributedLockId>(),
                    timeToLive,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var (success, distributedLock) = await facade.TryAcquireAsync(
                resourceId,
                timeToLive,
                CancellationToken.None);
            
            Assert.False(success);
            Assert.Null(distributedLock);
        }

        [Theory]
        [AutoMoqData]
        public async Task ReturnLock_When_TryingToAcquire_And_RepositorySucceeds(
            DistributedLockResourceId resourceId,
            DistributedLockTimeToLive timeToLive,
            [Frozen] Mock<IDistributedLocksRepository> repositoryMock,
            DistributedLockFacade facade)
        {
            DistributedLockId lockId = null;
            repositoryMock
                .Setup(lockRepository => lockRepository.TryInsert(
                    resourceId,
                    It.IsAny<DistributedLockId>(),
                    timeToLive,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .Callback<DistributedLockResourceId, DistributedLockId, DistributedLockTimeToLive, CancellationToken>(
                    (_, id, _, _) => lockId = id);

            var (success, distributedLock) = await facade.TryAcquireAsync(
                resourceId,
                timeToLive,
                CancellationToken.None);
            
            Assert.True(success);
            Assert.NotNull(distributedLock);
            Assert.NotNull(lockId);
            Assert.Equal(lockId, distributedLock.Id);
            Assert.Equal(resourceId, distributedLock.ResourceId);
        }

        [Theory]
        [AutoMoqData]
        public async Task Throw_When_TryingToExtend_And_ResourceIdIsNull(
            DistributedLockId id,
            DistributedLockTimeToLive timeToLive,
            DistributedLockFacade facade)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => facade.TryExtendAsync(
                null,
                id,
                timeToLive,
                CancellationToken.None));
        }

        [Theory]
        [AutoMoqData]
        public async Task Throw_When_TryingToExtend_And_IdIsNull(
            DistributedLockResourceId resourceId,
            DistributedLockTimeToLive timeToLive,
            DistributedLockFacade facade)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => facade.TryExtendAsync(
                resourceId,
                null,
                timeToLive,
                CancellationToken.None));
        }

        [Theory]
        [AutoMoqData]
        public async Task Throw_When_TryingToExtend_And_TimeToLiveIsNull(
            DistributedLockResourceId resourceId,
            DistributedLockId id,
            DistributedLockFacade facade)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => facade.TryExtendAsync(
                resourceId,
                id,
                null,
                CancellationToken.None));
        }

        [Theory]
        [AutoMoqWithInlineData(true)]
        [AutoMoqWithInlineData(false)]
        public async Task ReturnRepositoryResult_When_TryingToExtend(
            bool expectedResult,
            DistributedLockResourceId resourceId,
            DistributedLockId id,
            DistributedLockTimeToLive timeToLive,
            [Frozen] Mock<IDistributedLocksRepository> repositoryMock,
            DistributedLockFacade facade)
        {
            repositoryMock
                .Setup(repository => repository.TryUpdateTimeToLiveAsync(
                    resourceId,
                    id,
                    timeToLive,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);
            
            var result = await facade.TryExtendAsync(
                resourceId,
                id,
                timeToLive,
                CancellationToken.None);
            
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [AutoMoqData]
        public async Task Throw_When_TryingToRelease_And_ResourceIdIsNull(
            DistributedLockId id,
            DistributedLockFacade facade)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => facade.TryReleaseAsync(
                null,
                id,
                CancellationToken.None));
        }

        [Theory]
        [AutoMoqData]
        public async Task Throw_When_TryingToRelease_And_IdIsNull(
            DistributedLockResourceId resourceId,
            DistributedLockFacade facade)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => facade.TryReleaseAsync(
                resourceId,
                null,
                CancellationToken.None));
        }

        [Theory]
        [AutoMoqWithInlineData(true)]
        [AutoMoqWithInlineData(false)]
        public async Task ReturnRepositoryResult_When_TryingToRelease(
            bool expectedResult,
            DistributedLockResourceId resourceId,
            DistributedLockId id,
            [Frozen] Mock<IDistributedLocksRepository> repositoryMock,
            DistributedLockFacade facade)
        {
            repositoryMock
                .Setup(repository => repository.TryDelete(
                    resourceId,
                    id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);
            
            var result = await facade.TryReleaseAsync(
                resourceId,
                id,
                CancellationToken.None);
            
            Assert.Equal(expectedResult, result);
        }
    }
}