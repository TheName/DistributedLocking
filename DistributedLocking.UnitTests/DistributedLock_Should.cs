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
        public void Throw_When_TryingToCreateWithNullLockId(
            DistributedLockIdentifier lockIdentifier,
            IDistributedLockRepository distributedLockRepository)
        {
            Assert.Throws<ArgumentNullException>(() => new DistributedLock(null, lockIdentifier, distributedLockRepository));
        }
        
        [Theory]
        [AutoMoqData]
        public void Throw_When_TryingToCreateWithNullLockIdentifier(
            DistributedLockId lockId,
            IDistributedLockRepository distributedLockRepository)
        {
            Assert.Throws<ArgumentNullException>(() => new DistributedLock(lockId, null, distributedLockRepository));
        }
        
        [Theory]
        [AutoMoqData]
        public void Throw_When_TryingToCreateWithNullRepository(
            DistributedLockId lockId,
            DistributedLockIdentifier lockIdentifier)
        {
            Assert.Throws<ArgumentNullException>(() => new DistributedLock(lockId, lockIdentifier, null));
        }
        
        [Theory]
        [AutoMoqData]
        public void NotThrow_When_TryingToCreateWithoutNullValues(
            DistributedLockId lockId,
            DistributedLockIdentifier lockIdentifier,
            IDistributedLockRepository repository)
        {
            _ = new DistributedLock(lockId, lockIdentifier, repository);
        }

        [Theory]
        [AutoMoqData]
        internal async Task TryToReleaseLock_When_Disposing(
            [Frozen] Mock<IDistributedLockRepository> distributedLockRepositoryMock,
            DistributedLock distributedLock)
        {
            await distributedLock.DisposeAsync();

            distributedLockRepositoryMock
                .Verify(
                    repository => repository.TryReleaseAsync(
                        distributedLock.LockIdentifier,
                        distributedLock.LockId,
                        CancellationToken.None),
                    Times.Once);
        }
    }
}