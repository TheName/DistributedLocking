using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using DistributedLocking.Abstractions;
using DistributedLocking.Abstractions.Facades.Exceptions;
using DistributedLocking.Abstractions.Repositories;
using DistributedLocking.Facades;
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
        public void NotThrow_When_Creating_And_AllParametersAreNotNull(IDistributedLockRepository repository)
        {
            _ = new DistributedLockFacade(repository);
        }

        [Theory]
        [AutoMoqData]
        public async Task Throw_When_Acquiring_And_IdentifierIsNull(
            DistributedLockTimeToLive timeToLive,
            DistributedLockFacade facade)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => facade.AcquireAsync(
                null,
                timeToLive,
                CancellationToken.None));
        }

        [Theory]
        [AutoMoqData]
        public async Task Throw_When_Acquiring_And_TimeToLiveIsNull(
            DistributedLockIdentifier identifier,
            DistributedLockFacade facade)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => facade.AcquireAsync(
                identifier,
                null,
                CancellationToken.None));
        }

        [Theory]
        [AutoMoqData]
        public async Task Throw_When_Acquiring_And_RepositoryFails(
            DistributedLockIdentifier identifier,
            DistributedLockTimeToLive timeToLive,
            [Frozen] Mock<IDistributedLockRepository> repositoryMock,
            DistributedLockFacade facade)
        {
            repositoryMock
                .Setup(lockRepository => lockRepository.TryInsertIfIdentifierNotExistsAsync(
                    identifier,
                    It.IsAny<DistributedLockId>(),
                    timeToLive,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var exception = await Assert.ThrowsAsync<CouldNotAcquireLockException>(() =>
                facade.AcquireAsync(
                    identifier,
                    timeToLive,
                    CancellationToken.None));
            
            Assert.Equal(identifier, exception.Identifier);
            Assert.Equal(timeToLive, exception.TimeToLive);
        }

        [Theory]
        [AutoMoqData]
        public async Task ReturnLock_When_Acquiring_And_RepositorySucceeds(
            DistributedLockIdentifier identifier,
            DistributedLockTimeToLive timeToLive,
            [Frozen] Mock<IDistributedLockRepository> repositoryMock,
            DistributedLockFacade facade)
        {
            DistributedLockId lockId = null;
            repositoryMock
                .Setup(lockRepository => lockRepository.TryInsertIfIdentifierNotExistsAsync(
                    identifier,
                    It.IsAny<DistributedLockId>(),
                    timeToLive,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .Callback<DistributedLockIdentifier, DistributedLockId, DistributedLockTimeToLive, CancellationToken>(
                    (_, id, _, _) => lockId = id);

            var result = await facade.AcquireAsync(
                identifier,
                timeToLive,
                CancellationToken.None);
            
            var distributedLock = Assert.IsType<DistributedLock>(result);
            Assert.NotNull(distributedLock);
            Assert.NotNull(lockId);
            Assert.Equal(lockId, distributedLock.Id);
            Assert.Equal(identifier, distributedLock.Identifier);
        }

        [Theory]
        [AutoMoqData]
        public async Task Throw_When_Extending_And_RepositoryFails(
            IDistributedLock distributedLock,
            DistributedLockTimeToLive timeToLive,
            [Frozen] Mock<IDistributedLockRepository> repositoryMock,
            DistributedLockFacade facade)
        {
            repositoryMock
                .Setup(lockRepository => lockRepository.TryUpdateTimeToLiveAsync(
                    distributedLock.Identifier,
                    distributedLock.Id,
                    timeToLive,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var exception = await Assert.ThrowsAsync<CouldNotExtendLockException>(() =>
                facade.ExtendAsync(
                    distributedLock,
                    timeToLive,
                    CancellationToken.None));
            
            Assert.Equal(distributedLock.Identifier, exception.Identifier);
            Assert.Equal(distributedLock.Id, exception.Id);
        }

        [Theory]
        [AutoMoqData]
        public async Task NotThrow_When_Extending_And_RepositorySucceeds(
            IDistributedLock distributedLock,
            DistributedLockTimeToLive additionalTimeToLive,
            [Frozen] Mock<IDistributedLockRepository> repositoryMock,
            DistributedLockFacade facade)
        {
            repositoryMock
                .Setup(lockRepository => lockRepository.TryUpdateTimeToLiveAsync(
                    distributedLock.Identifier,
                    distributedLock.Id,
                    additionalTimeToLive,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await facade.ExtendAsync(
                distributedLock,
                additionalTimeToLive,
                CancellationToken.None);

            repositoryMock
                .Verify(repository => repository.TryUpdateTimeToLiveAsync(
                        distributedLock.Identifier,
                        distributedLock.Id,
                        additionalTimeToLive,
                        It.IsAny<CancellationToken>()),
                    Times.Once);
            repositoryMock.VerifyNoOtherCalls();
        }

        [Theory]
        [AutoMoqData]
        public async Task Throw_When_Releasing_And_RepositoryFails(
            IDistributedLock distributedLock,
            [Frozen] Mock<IDistributedLockRepository> repositoryMock,
            DistributedLockFacade facade)
        {
            repositoryMock
                .Setup(lockRepository => lockRepository.TryDeleteIfExistsAsync(
                    distributedLock.Identifier,
                    distributedLock.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var exception = await Assert.ThrowsAsync<CouldNotReleaseLockException>(() =>
                facade.ReleaseAsync(
                    distributedLock,
                    CancellationToken.None));
            
            Assert.Equal(distributedLock.Identifier, exception.Identifier);
            Assert.Equal(distributedLock.Id, exception.Id);
        }

        [Theory]
        [AutoMoqData]
        public async Task NotThrow_When_Releasing_And_RepositorySucceeds(
            IDistributedLock distributedLock,
            [Frozen] Mock<IDistributedLockRepository> repositoryMock,
            DistributedLockFacade facade)
        {
            repositoryMock
                .Setup(lockRepository => lockRepository.TryDeleteIfExistsAsync(
                    distributedLock.Identifier,
                    distributedLock.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await facade.ReleaseAsync(
                distributedLock,
                CancellationToken.None);

            repositoryMock
                .Verify(repository => repository.TryDeleteIfExistsAsync(
                        distributedLock.Identifier,
                        distributedLock.Id,
                        It.IsAny<CancellationToken>()),
                    Times.Once);
            repositoryMock.VerifyNoOtherCalls();
        }
    }
}