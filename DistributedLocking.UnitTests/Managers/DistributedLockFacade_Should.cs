using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using DistributedLocking.Abstractions;
using DistributedLocking.Abstractions.Exceptions;
using DistributedLocking.Abstractions.Records;
using DistributedLocking.Abstractions.Repositories;
using DistributedLocking.Abstractions.Retries;
using DistributedLocking.Facades;
using DistributedLocking.UnitTests.Extensions;
using Moq;
using TestHelpers.Attributes;
using Xunit;

namespace DistributedLocking.UnitTests.Managers
{
    public class DistributedLockFacade_Should
    {
        [Theory]
        [AutoMoqData]
        public void Throw_When_Creating_And_RepositoryIsNull(IRetryExecutor retryExecutor)
        {
            Assert.Throws<ArgumentNullException>(() => new DistributedLockFacade(null, retryExecutor));
        }
        
        [Theory]
        [AutoMoqData]
        public void Throw_When_Creating_And_RetryExecutorIsNull(IDistributedLockRepository repository)
        {
            Assert.Throws<ArgumentNullException>(() => new DistributedLockFacade(repository, null));
        }
        
        [Theory]
        [AutoMoqData]
        public void NotThrow_When_Creating_And_RetryExecutorIsNull(
            IDistributedLockRepository repository,
            IRetryExecutor retryExecutor)
        {
            _ = new DistributedLockFacade(repository, retryExecutor);
        }

        [Theory]
        [AutoMoqData]
        public async Task Throw_When_Acquiring_And_IdentifierIsNull(
            DistributedLockTimeToLive timeToLive,
            IRetryPolicy retryPolicy,
            DistributedLockFacade manager)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => manager.AcquireAsync(
                null,
                timeToLive,
                retryPolicy,
                CancellationToken.None));
        }

        [Theory]
        [AutoMoqData]
        public async Task Throw_When_Acquiring_And_TimeToLiveIsNull(
            DistributedLockIdentifier identifier,
            IRetryPolicy retryPolicy,
            DistributedLockFacade manager)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => manager.AcquireAsync(
                identifier,
                null,
                retryPolicy,
                CancellationToken.None));
        }

        [Theory]
        [AutoMoqData]
        public async Task Throw_When_Acquiring_And_RetryPolicyProviderIsNull(
            DistributedLockIdentifier identifier,
            DistributedLockTimeToLive timeToLive,
            DistributedLockFacade manager)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => manager.AcquireAsync(
                identifier,
                timeToLive,
                null,
                CancellationToken.None));
        }

        [Theory]
        [AutoMoqData]
        public async Task Throw_When_Acquiring_And_RetryExecutorThrows(
            DistributedLockIdentifier identifier,
            DistributedLockTimeToLive timeToLive,
            IRetryPolicy retryPolicy,
            Exception exceptionToThrow,
            [Frozen] Mock<IRetryExecutor> retryExecutorMock,
            DistributedLockFacade manager)
        {
            retryExecutorMock.SetupException<IDistributedLock>(exceptionToThrow);

            var exception = await Assert.ThrowsAsync<CouldNotAcquireLockException>(() =>
                manager.AcquireAsync(
                    identifier,
                    timeToLive,
                    retryPolicy,
                    CancellationToken.None));
            
            Assert.Equal(identifier, exception.Identifier);
            Assert.Equal(timeToLive, exception.TimeToLive);
            Assert.Equal(exceptionToThrow, exception.InnerException);
        }

        [Theory]
        [AutoMoqData]
        public async Task ReturnLock_When_Acquiring_And_RepositorySucceeds(
            DistributedLockIdentifier identifier,
            DistributedLockTimeToLive timeToLive,
            IRetryPolicy retryPolicy,
            DistributedLockId lockId,
            [Frozen] Mock<IDistributedLockRepository> repositoryMock,
            [Frozen] Mock<IRetryExecutor> retryExecutorMock,
            DistributedLockFacade manager)
        {
            retryExecutorMock.Setup<IDistributedLock>();
            repositoryMock
                .Setup(lockRepository => lockRepository.TryAcquireAsync(
                    identifier,
                    timeToLive,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((true, lockId));

            var result = await manager.AcquireAsync(
                identifier,
                timeToLive,
                retryPolicy,
                CancellationToken.None);
            
            var distributedLock = Assert.IsType<DistributedLock>(result);
            Assert.NotNull(distributedLock);
            Assert.Equal(lockId, distributedLock.Id);
            Assert.Equal(identifier, distributedLock.Identifier);
        }

        [Theory]
        [AutoMoqData]
        public async Task Throw_When_Releasing_And_RetryExecutorThrows(
            IDistributedLock distributedLock,
            IRetryPolicy retryPolicy,
            Exception exceptionToThrow,
            [Frozen] Mock<IRetryExecutor> retryExecutorMock,
            DistributedLockFacade manager)
        {
            retryExecutorMock.SetupException(exceptionToThrow);

            var exception = await Assert.ThrowsAsync<CouldNotReleaseLockException>(() =>
                manager.ReleaseAsync(
                    distributedLock,
                    retryPolicy,
                    CancellationToken.None));
            
            Assert.Equal(distributedLock.Identifier, exception.Identifier);
            Assert.Equal(distributedLock.Id, exception.Id);
            Assert.Equal(exceptionToThrow, exception.InnerException);
        }

        [Theory]
        [AutoMoqData]
        public async Task NotThrow_When_Releasing_And_RepositorySucceeds(
            IDistributedLock distributedLock,
            IRetryPolicy retryPolicy,
            [Frozen] Mock<IDistributedLockRepository> repositoryMock,
            [Frozen] Mock<IRetryExecutor> retryExecutorMock,
            DistributedLockFacade manager)
        {
            retryExecutorMock.Setup();
            repositoryMock
                .Setup(lockRepository => lockRepository.TryReleaseAsync(
                    distributedLock.Identifier,
                    distributedLock.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await manager.ReleaseAsync(
                distributedLock,
                retryPolicy,
                CancellationToken.None);

            repositoryMock
                .Verify(repository => repository.TryReleaseAsync(
                        distributedLock.Identifier,
                        distributedLock.Id,
                        It.IsAny<CancellationToken>()),
                    Times.Once);
            repositoryMock.VerifyNoOtherCalls();
        }

        [Theory]
        [AutoMoqData]
        public async Task Throw_When_Extending_And_RetryExecutorThrows(
            IDistributedLock distributedLock,
            DistributedLockTimeToLive timeToLive,
            IRetryPolicy retryPolicy,
            Exception exceptionToThrow,
            [Frozen] Mock<IRetryExecutor> retryExecutorMock,
            DistributedLockFacade manager)
        {
            retryExecutorMock.SetupException(exceptionToThrow);

            var exception = await Assert.ThrowsAsync<CouldNotExtendLockException>(() =>
                manager.ExtendAsync(
                    distributedLock,
                    timeToLive,
                    retryPolicy,
                    CancellationToken.None));
            
            Assert.Equal(distributedLock.Identifier, exception.Identifier);
            Assert.Equal(distributedLock.Id, exception.Id);
            Assert.Equal(exceptionToThrow, exception.InnerException);
        }

        [Theory]
        [AutoMoqData]
        public async Task NotThrow_When_Extending_And_RepositorySucceeds(
            IDistributedLock distributedLock,
            DistributedLockTimeToLive additionalTimeToLive,
            IRetryPolicy retryPolicy,
            [Frozen] Mock<IDistributedLockRepository> repositoryMock,
            [Frozen] Mock<IRetryExecutor> retryExecutorMock,
            DistributedLockFacade manager)
        {
            retryExecutorMock.Setup();
            repositoryMock
                .Setup(lockRepository => lockRepository.TryExtendAsync(
                    distributedLock.Identifier,
                    distributedLock.Id,
                    additionalTimeToLive,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await manager.ExtendAsync(
                distributedLock,
                additionalTimeToLive,
                retryPolicy,
                CancellationToken.None);

            repositoryMock
                .Verify(repository => repository.TryExtendAsync(
                        distributedLock.Identifier,
                        distributedLock.Id,
                        additionalTimeToLive,
                        It.IsAny<CancellationToken>()),
                    Times.Once);
            repositoryMock.VerifyNoOtherCalls();
        }
    }
}