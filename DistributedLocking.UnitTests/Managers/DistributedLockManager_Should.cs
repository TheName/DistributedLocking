﻿using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using DistributedLocking.Abstractions;
using DistributedLocking.Abstractions.Exceptions;
using DistributedLocking.Abstractions.Records;
using DistributedLocking.Abstractions.Repositories;
using DistributedLocking.Abstractions.Retries;
using DistributedLocking.Managers;
using Moq;
using TestHelpers.Attributes;
using Xunit;

namespace DistributedLocking.UnitTests.Managers
{
    public class DistributedLockManager_Should
    {
        [Theory]
        [AutoMoqData]
        public void Throw_When_Creating_And_RepositoryIsNull(IRetryExecutor retryExecutor)
        {
            Assert.Throws<ArgumentNullException>(() => new DistributedLockManager(null, retryExecutor));
        }
        
        [Theory]
        [AutoMoqData]
        public void Throw_When_Creating_And_RetryExecutorIsNull(IDistributedLockRepository repository)
        {
            Assert.Throws<ArgumentNullException>(() => new DistributedLockManager(repository, null));
        }
        
        [Theory]
        [AutoMoqData]
        public void NotThrow_When_Creating_And_RetryExecutorIsNull(
            IDistributedLockRepository repository,
            IRetryExecutor retryExecutor)
        {
            _ = new DistributedLockManager(repository, retryExecutor);
        }

        [Theory]
        [AutoMoqData]
        public async Task Throw_When_Acquiring_And_LockIdentifierIsNull(
            DistributedLockTimeToLive timeToLive,
            IRetryPolicyProvider retryPolicyProvider,
            DistributedLockManager manager)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => manager.AcquireAsync(
                null,
                timeToLive,
                retryPolicyProvider,
                CancellationToken.None));
        }

        [Theory]
        [AutoMoqData]
        public async Task Throw_When_Acquiring_And_TimeToLiveIsNull(
            DistributedLockIdentifier lockIdentifier,
            IRetryPolicyProvider retryPolicyProvider,
            DistributedLockManager manager)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => manager.AcquireAsync(
                lockIdentifier,
                null,
                retryPolicyProvider,
                CancellationToken.None));
        }

        [Theory]
        [AutoMoqData]
        public async Task Throw_When_Acquiring_And_RetryPolicyProviderIsNull(
            DistributedLockIdentifier lockIdentifier,
            DistributedLockTimeToLive timeToLive,
            DistributedLockManager manager)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => manager.AcquireAsync(
                lockIdentifier,
                timeToLive,
                null,
                CancellationToken.None));
        }

        [Theory]
        [AutoMoqData]
        public async Task Throw_When_Acquiring_And_RetryExecutorThrows(
            DistributedLockIdentifier identifier,
            DistributedLockTimeToLive timeToLive,
            IRetryPolicyProvider retryPolicyProvider,
            Exception exceptionToThrow,
            [Frozen] Mock<IRetryExecutor> retryExecutorMock,
            DistributedLockManager manager)
        {
            retryExecutorMock
                .Setup(executor => executor.ExecuteWithRetriesAsync(
                    It.IsAny<Func<Task<(bool, IDistributedLock)>>>(),
                    retryPolicyProvider,
                    It.IsAny<CancellationToken>()))
                .Throws(exceptionToThrow);

            var exception = await Assert.ThrowsAsync<CouldNotAcquireLockException>(() =>
                manager.AcquireAsync(
                    identifier,
                    timeToLive,
                    retryPolicyProvider,
                    CancellationToken.None));
            
            Assert.Equal(identifier, exception.LockIdentifier);
            Assert.Equal(timeToLive, exception.LockTimeToLive);
            Assert.Equal(exceptionToThrow, exception.InnerException);
        }

        [Theory]
        [AutoMoqData]
        public async Task ReturnLock_When_Acquiring_And_RepositorySucceeds(
            DistributedLockIdentifier identifier,
            DistributedLockTimeToLive timeToLive,
            IRetryPolicyProvider retryPolicyProvider,
            DistributedLockId lockId,
            [Frozen] Mock<IDistributedLockRepository> repositoryMock,
            [Frozen] Mock<IRetryExecutor> retryExecutorMock,
            DistributedLockManager manager)
        {
            repositoryMock
                .Setup(lockRepository => lockRepository.TryAcquireAsync(
                    identifier,
                    timeToLive,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((true, lockId));

            retryExecutorMock
                .Setup(executor => executor.ExecuteWithRetriesAsync(
                    It.IsAny<Func<Task<(bool, IDistributedLock)>>>(),
                    retryPolicyProvider,
                    It.IsAny<CancellationToken>()))
                .Returns<Func<Task<(bool, IDistributedLock)>>, IRetryPolicyProvider, CancellationToken>(
                    async (func, _, _) =>
                    {
                        var (_, @lock) = await func();
                        return @lock;
                    });

            var result = await manager.AcquireAsync(
                identifier,
                timeToLive,
                retryPolicyProvider,
                CancellationToken.None);
            
            var distributedLock = Assert.IsType<DistributedLock>(result);
            Assert.NotNull(distributedLock);
            Assert.Equal(lockId, distributedLock.LockId);
            Assert.Equal(identifier, distributedLock.LockIdentifier);
        }

        [Theory]
        [AutoMoqData]
        public async Task Throw_When_Releasing_And_RetryExecutorThrows(
            IDistributedLock distributedLock,
            IRetryPolicyProvider retryPolicyProvider,
            Exception exceptionToThrow,
            [Frozen] Mock<IRetryExecutor> retryExecutorMock,
            DistributedLockManager manager)
        {
            retryExecutorMock
                .Setup(executor => executor.ExecuteWithRetriesAsync(
                    It.IsAny<Func<Task<bool>>>(),
                    retryPolicyProvider,
                    It.IsAny<CancellationToken>()))
                .Throws(exceptionToThrow);

            var exception = await Assert.ThrowsAsync<CouldNotReleaseLockException>(() =>
                manager.ReleaseAsync(
                    distributedLock,
                    retryPolicyProvider,
                    CancellationToken.None));
            
            Assert.Equal(distributedLock.LockIdentifier, exception.LockIdentifier);
            Assert.Equal(distributedLock.LockId, exception.LockId);
            Assert.Equal(exceptionToThrow, exception.InnerException);
        }

        [Theory]
        [AutoMoqData]
        public async Task NotThrow_When_Releasing_And_RepositorySucceeds(
            IDistributedLock distributedLock,
            IRetryPolicyProvider retryPolicyProvider,
            [Frozen] Mock<IDistributedLockRepository> repositoryMock,
            [Frozen] Mock<IRetryExecutor> retryExecutorMock,
            DistributedLockManager manager)
        {
            retryExecutorMock
                .Setup(executor => executor.ExecuteWithRetriesAsync(
                    It.IsAny<Func<Task<bool>>>(),
                    retryPolicyProvider,
                    It.IsAny<CancellationToken>()))
                .Returns<Func<Task<bool>>, IRetryPolicyProvider, CancellationToken>(
                    async (func, _, _) =>
                    {
                        if (!await func())
                        {
                            throw new Exception();
                        }
                    });
            
            repositoryMock
                .Setup(lockRepository => lockRepository.TryReleaseAsync(
                    distributedLock.LockIdentifier,
                    distributedLock.LockId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await manager.ReleaseAsync(
                distributedLock,
                retryPolicyProvider,
                CancellationToken.None);

            repositoryMock
                .Verify(repository => repository.TryReleaseAsync(
                        distributedLock.LockIdentifier,
                        distributedLock.LockId,
                        It.IsAny<CancellationToken>()),
                    Times.Once);
            repositoryMock.VerifyNoOtherCalls();
        }

        [Theory]
        [AutoMoqData]
        public async Task Throw_When_Extending_And_RetryExecutorThrows(
            IDistributedLock distributedLock,
            DistributedLockTimeToLive timeToLive,
            IRetryPolicyProvider retryPolicyProvider,
            Exception exceptionToThrow,
            [Frozen] Mock<IRetryExecutor> retryExecutorMock,
            DistributedLockManager manager)
        {
            retryExecutorMock
                .Setup(executor => executor.ExecuteWithRetriesAsync(
                    It.IsAny<Func<Task<bool>>>(),
                    retryPolicyProvider,
                    It.IsAny<CancellationToken>()))
                .Throws(exceptionToThrow);

            var exception = await Assert.ThrowsAsync<CouldNotExtendLockException>(() =>
                manager.ExtendAsync(
                    distributedLock,
                    timeToLive,
                    retryPolicyProvider,
                    CancellationToken.None));
            
            Assert.Equal(distributedLock.LockIdentifier, exception.LockIdentifier);
            Assert.Equal(distributedLock.LockId, exception.LockId);
            Assert.Equal(exceptionToThrow, exception.InnerException);
        }

        [Theory]
        [AutoMoqData]
        public async Task NotThrow_When_Extending_And_RepositorySucceeds(
            IDistributedLock distributedLock,
            DistributedLockTimeToLive additionalTimeToLive,
            IRetryPolicyProvider retryPolicyProvider,
            [Frozen] Mock<IDistributedLockRepository> repositoryMock,
            [Frozen] Mock<IRetryExecutor> retryExecutorMock,
            DistributedLockManager manager)
        {
            repositoryMock
                .Setup(lockRepository => lockRepository.TryExtendAsync(
                    distributedLock.LockIdentifier,
                    distributedLock.LockId,
                    additionalTimeToLive,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            
            retryExecutorMock
                .Setup(executor => executor.ExecuteWithRetriesAsync(
                    It.IsAny<Func<Task<bool>>>(),
                    retryPolicyProvider,
                    It.IsAny<CancellationToken>()))
                .Returns<Func<Task<bool>>, IRetryPolicyProvider, CancellationToken>(
                    async (func, _, _) =>
                    {
                        if (!await func())
                        {
                            throw new Exception();
                        }
                    });

            await manager.ExtendAsync(
                distributedLock,
                additionalTimeToLive,
                retryPolicyProvider,
                CancellationToken.None);

            repositoryMock
                .Verify(repository => repository.TryExtendAsync(
                        distributedLock.LockIdentifier,
                        distributedLock.LockId,
                        additionalTimeToLive,
                        It.IsAny<CancellationToken>()),
                    Times.Once);
            repositoryMock.VerifyNoOtherCalls();
        }
    }
}