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
        public async Task Throw_When_TryingToAcquire_And_IdentifierIsNull(
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
            DistributedLockIdentifier identifier,
            DistributedLockFacade facade)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => facade.TryAcquireAsync(
                identifier,
                null,
                CancellationToken.None));
        }

        [Theory]
        [AutoMoqData]
        public async Task ReturnNull_When_TryingToAcquire_And_RepositoryFails(
            DistributedLockIdentifier identifier,
            DistributedLockTimeToLive timeToLive,
            [Frozen] Mock<IDistributedLocksRepository> repositoryMock,
            DistributedLockFacade facade)
        {
            repositoryMock
                .Setup(lockRepository => lockRepository.TryInsert(
                    identifier,
                    It.IsAny<DistributedLockId>(),
                    timeToLive,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var (success, distributedLock) = await facade.TryAcquireAsync(
                identifier,
                timeToLive,
                CancellationToken.None);
            
            Assert.False(success);
            Assert.Null(distributedLock);
        }

        [Theory]
        [AutoMoqData]
        public async Task ReturnLock_When_TryingToAcquire_And_RepositorySucceeds(
            DistributedLockIdentifier identifier,
            DistributedLockTimeToLive timeToLive,
            [Frozen] Mock<IDistributedLocksRepository> repositoryMock,
            DistributedLockFacade facade)
        {
            DistributedLockId lockId = null;
            repositoryMock
                .Setup(lockRepository => lockRepository.TryInsert(
                    identifier,
                    It.IsAny<DistributedLockId>(),
                    timeToLive,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .Callback<DistributedLockIdentifier, DistributedLockId, DistributedLockTimeToLive, CancellationToken>(
                    (_, id, _, _) => lockId = id);

            var (success, distributedLock) = await facade.TryAcquireAsync(
                identifier,
                timeToLive,
                CancellationToken.None);
            
            Assert.True(success);
            Assert.NotNull(distributedLock);
            Assert.NotNull(lockId);
            Assert.Equal(lockId, distributedLock.Id);
            Assert.Equal(identifier, distributedLock.Identifier);
        }

        [Theory]
        [AutoMoqData]
        public async Task Throw_When_TryingToExtend_And_IdentifierIsNull(
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
            DistributedLockIdentifier identifier,
            DistributedLockTimeToLive timeToLive,
            DistributedLockFacade facade)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => facade.TryExtendAsync(
                identifier,
                null,
                timeToLive,
                CancellationToken.None));
        }

        [Theory]
        [AutoMoqData]
        public async Task Throw_When_TryingToExtend_And_TimeToLiveIsNull(
            DistributedLockIdentifier identifier,
            DistributedLockId id,
            DistributedLockFacade facade)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => facade.TryExtendAsync(
                identifier,
                id,
                null,
                CancellationToken.None));
        }

        [Theory]
        [AutoMoqWithInlineData(true)]
        [AutoMoqWithInlineData(false)]
        public async Task ReturnRepositoryResult_When_TryingToExtend(
            bool expectedResult,
            DistributedLockIdentifier identifier,
            DistributedLockId id,
            DistributedLockTimeToLive timeToLive,
            [Frozen] Mock<IDistributedLocksRepository> repositoryMock,
            DistributedLockFacade facade)
        {
            repositoryMock
                .Setup(repository => repository.TryUpdateTimeToLiveAsync(
                    identifier,
                    id,
                    timeToLive,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);
            
            var result = await facade.TryExtendAsync(
                identifier,
                id,
                timeToLive,
                CancellationToken.None);
            
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [AutoMoqData]
        public async Task Throw_When_TryingToRelease_And_IdentifierIsNull(
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
            DistributedLockIdentifier identifier,
            DistributedLockFacade facade)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => facade.TryReleaseAsync(
                identifier,
                null,
                CancellationToken.None));
        }

        [Theory]
        [AutoMoqWithInlineData(true)]
        [AutoMoqWithInlineData(false)]
        public async Task ReturnRepositoryResult_When_TryingToRelease(
            bool expectedResult,
            DistributedLockIdentifier identifier,
            DistributedLockId id,
            [Frozen] Mock<IDistributedLocksRepository> repositoryMock,
            DistributedLockFacade facade)
        {
            repositoryMock
                .Setup(repository => repository.TryDelete(
                    identifier,
                    id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);
            
            var result = await facade.TryReleaseAsync(
                identifier,
                id,
                CancellationToken.None);
            
            Assert.Equal(expectedResult, result);
        }
    }
}