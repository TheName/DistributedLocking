using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Moq;
using TestHelpers.Attributes;
using TheName.DistributedLocking;
using TheName.DistributedLocking.Abstractions;
using TheName.DistributedLocking.Abstractions.Exceptions;
using TheName.DistributedLocking.Abstractions.Records;
using TheName.DistributedLocking.Abstractions.Repositories;
using TheName.DistributedLocking.Managers;
using Xunit;

namespace DistributedLocking.UnitTests.Managers
{
    public class DistributedLockManager_Should
    {
        [Fact]
        public void Throw_When_Creating_And_RepositoryIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new DistributedLockManager(null));
        }

        [Theory]
        [AutoMoqData]
        public async Task Throw_When_TryingToAcquire_And_RepositoryFails(
            DistributedLockIdentifier identifier,
            DistributedLockTimeToLive timeToLive,
            DistributedLockAcquiringTimeout acquiringTimeout,
            DistributedLockAcquiringDelayBetweenRetries delayBetweenRetries,
            [Frozen] IDistributedLockRepository repository,
            DistributedLockManager manager)
        {
            Mock.Get(repository)
                .Setup(lockRepository => lockRepository.TryAcquireAsync(
                    It.IsAny<DistributedLockIdentifier>(),
                    It.IsAny<DistributedLockTimeToLive>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((false, null));

            await Assert.ThrowsAsync<CouldNotAcquireLockException>(() => manager.AcquireAsync(
                identifier,
                timeToLive,
                acquiringTimeout,
                delayBetweenRetries,
                CancellationToken.None));
        }

        [Theory]
        [AutoMoqData]
        public async Task ReturnLock_When_TryingToAcquire_And_RepositorySucceeds(
            DistributedLockIdentifier identifier,
            DistributedLockTimeToLive timeToLive,
            DistributedLockAcquiringTimeout acquiringTimeout,
            DistributedLockAcquiringDelayBetweenRetries delayBetweenRetries,
            DistributedLockId lockId,
            [Frozen] IDistributedLockRepository repository,
            DistributedLockManager manager)
        {
            Mock.Get(repository)
                .Setup(lockRepository => lockRepository.TryAcquireAsync(
                    It.IsAny<DistributedLockIdentifier>(),
                    It.IsAny<DistributedLockTimeToLive>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((true, lockId));

            var result = await manager.AcquireAsync(
                identifier,
                timeToLive,
                acquiringTimeout,
                delayBetweenRetries,
                CancellationToken.None);
            
            var distributedLock = Assert.IsType<DistributedLock>(result);
            Assert.NotNull(distributedLock);
            Assert.Equal(lockId, distributedLock.LockId);
            Assert.Equal(identifier, distributedLock.LockIdentifier);
        }

        [Theory]
        [AutoMoqData]
        public async Task RetryAcquiring_When_TryingToAcquire_And_RepositoryFails(
            DistributedLockIdentifier identifier,
            DistributedLockTimeToLive timeToLive,
            [Frozen] IDistributedLockRepository repository,
            DistributedLockManager manager)
        {
            Mock.Get(repository)
                .Setup(lockRepository => lockRepository.TryAcquireAsync(
                    It.IsAny<DistributedLockIdentifier>(),
                    It.IsAny<DistributedLockTimeToLive>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((false, null));

            const int expectedNumberOfRepositoryCalls = 10;
            var delay = new DistributedLockAcquiringDelayBetweenRetries(TimeSpan.FromMilliseconds(100));
            var totalTimeout = new DistributedLockAcquiringTimeout(delay.Value * 10 + delay.Value / 2);

            await Assert.ThrowsAsync<CouldNotAcquireLockException>(() => manager.AcquireAsync(
                identifier,
                timeToLive,
                totalTimeout,
                delay,
                CancellationToken.None));

            Mock.Get(repository)
                .Verify(lockRepository => lockRepository.TryAcquireAsync(
                        It.IsAny<DistributedLockIdentifier>(),
                        It.IsAny<DistributedLockTimeToLive>(),
                        It.IsAny<CancellationToken>()),
                    Times.Exactly(expectedNumberOfRepositoryCalls));
        }

        [Theory]
        [AutoMoqData]
        public async Task Throw_When_TryingToRelease_And_RepositoryFails(
            IDistributedLock distributedLock,
            [Frozen] IDistributedLockRepository repository,
            DistributedLockManager manager)
        {
            Mock.Get(repository)
                .Setup(lockRepository => lockRepository.TryReleaseAsync(
                    It.IsAny<DistributedLockId>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            await Assert.ThrowsAsync<CouldNotReleaseLockException>(() => manager.ReleaseAsync(
                distributedLock,
                CancellationToken.None));
        }

        [Theory]
        [AutoMoqData]
        public async Task NotThrow_When_TryingToRelease_And_RepositorySucceeds(
            IDistributedLock distributedLock,
            [Frozen] IDistributedLockRepository repository,
            DistributedLockManager manager)
        {
            Mock.Get(repository)
                .Setup(lockRepository => lockRepository.TryReleaseAsync(
                    distributedLock.LockId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await manager.ReleaseAsync(distributedLock, CancellationToken.None);
        }

        [Theory]
        [AutoMoqData]
        public async Task Throw_When_TryingToExtend_And_RepositoryFails(
            IDistributedLock distributedLock,
            DistributedLockTimeToLive additionalTimeToLive,
            [Frozen] IDistributedLockRepository repository,
            DistributedLockManager manager)
        {
            Mock.Get(repository)
                .Setup(lockRepository => lockRepository.TryExtendAsync(
                    It.IsAny<DistributedLockId>(),
                    It.IsAny<DistributedLockTimeToLive>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            await Assert.ThrowsAsync<CouldNotExtendLockException>(() => manager.ExtendAsync(
                distributedLock,
                additionalTimeToLive,
                CancellationToken.None));
        }

        [Theory]
        [AutoMoqData]
        public async Task NotThrow_When_TryingToExtend_And_RepositorySucceeds(
            IDistributedLock distributedLock,
            DistributedLockTimeToLive additionalTimeToLive,
            [Frozen] IDistributedLockRepository repository,
            DistributedLockManager manager)
        {
            Mock.Get(repository)
                .Setup(lockRepository => lockRepository.TryExtendAsync(
                    distributedLock.LockId,
                    additionalTimeToLive,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await manager.ReleaseAsync(distributedLock, CancellationToken.None);
        }
    }
}