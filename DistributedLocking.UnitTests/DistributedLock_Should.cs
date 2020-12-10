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
    public class DistributedLock_Should
    {
        [Theory]
        [AutoMoqData]
        public void Throw_When_TryingToCreateWithNullLockId(IDistributedLockRepository distributedLockRepository)
        {
            Assert.Throws<ArgumentNullException>(() => new DistributedLock(null, distributedLockRepository));
        }
        
        [Theory]
        [AutoMoqData]
        public void Throw_When_TryingToCreateWithNullRepository(LockId lockId)
        {
            Assert.Throws<ArgumentNullException>(() => new DistributedLock(lockId, null));
        }
        
        [Theory]
        [AutoMoqData]
        public void NotThrow_When_TryingToCreateWithoutNullValues(
            LockId lockId,
            IDistributedLockRepository repository)
        {
            _ = new DistributedLock(lockId, repository);
        }

        [Theory]
        [AutoMoqData]
        public async Task TryToReleaseLock_When_Disposing(
            [Frozen] Mock<IDistributedLockRepository> distributedLockRepositoryMock,
            DistributedLock distributedLock)
        {
            await distributedLock.DisposeAsync();

            distributedLockRepositoryMock
                .Verify(
                    repository => repository.TryReleaseAsync(distributedLock.LockId, CancellationToken.None),
                    Times.Once);
        }

        [Theory]
        [AutoMoqData]
        public async Task Throw_When_TryingToReleaseLock_When_Disposing_And_ReleasingFails(
            [Frozen] Mock<IDistributedLockRepository> distributedLockRepositoryMock,
            DistributedLock distributedLock)
        {
            distributedLockRepositoryMock
                .Setup(repository => repository.TryReleaseAsync(distributedLock.LockId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            await Assert.ThrowsAsync<CouldNotReleaseLockException>(async () => await distributedLock.DisposeAsync());
        }
    }
}