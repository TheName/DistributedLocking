using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Moq;
using TestHelpers.Attributes;
using TheName.DistributedLocking;
using TheName.DistributedLocking.Abstractions.Exceptions;
using TheName.DistributedLocking.Abstractions.Records;
using TheName.DistributedLocking.Abstractions.Repositories;
using Xunit;

namespace DistributedLocking.UnitTests
{
    public class DistributedLockAcquirer_Should
    {
        [Fact]
        public void Throw_When_TryingToCreateWithNullRepository()
        {
            Assert.Throws<ArgumentNullException>(() => new DistributedLockAcquirer(null));
        }
        
        [Theory]
        [AutoMoqData]
        public void NotThrow_When_TryingToCreateWithNotNullRepository(IDistributedLockRepository repository)
        {
            _ = new DistributedLockAcquirer(repository);
        }

        [Theory]
        [AutoMoqData]
        public async Task Throw_When_TryingToAcquireLock_And_RepositoryFails(
            DistributedLockIdentifier lockIdentifier,
            DistributedLockTimeToLive lockTimeToLive,
            [Frozen] Mock<IDistributedLockRepository> repositoryMock,
            DistributedLockAcquirer distributedLockAcquirer,
            CancellationToken cancellationToken)
        {
            repositoryMock
                .Setup(repository => repository.TryAcquireAsync(
                    lockIdentifier,
                    lockTimeToLive,
                    cancellationToken))
                .ReturnsAsync((false, null));

            await Assert.ThrowsAsync<CouldNotAcquireLockException>(() =>
                distributedLockAcquirer.AcquireAsync(
                    lockIdentifier,
                    lockTimeToLive,
                    cancellationToken));
        }

        [Theory]
        [AutoMoqData]
        public async Task ReturnDistributedLock_When_TryingToAcquireLock_And_RepositorySucceeds(
            DistributedLockIdentifier lockIdentifier,
            DistributedLockTimeToLive lockTimeToLive,
            DistributedLockId lockId,
            [Frozen] Mock<IDistributedLockRepository> repositoryMock,
            DistributedLockAcquirer distributedLockAcquirer,
            CancellationToken cancellationToken)
        {
            repositoryMock
                .Setup(repository => repository.TryAcquireAsync(
                    lockIdentifier,
                    lockTimeToLive,
                    cancellationToken))
                .ReturnsAsync((true, lockId));

            var result = await distributedLockAcquirer.AcquireAsync(
                lockIdentifier,
                lockTimeToLive,
                cancellationToken);

            Assert.Equal(lockId, result.LockId);
        }
    }
}