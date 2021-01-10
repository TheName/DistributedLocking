using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using DistributedLocking.Abstractions.Records;
using DistributedLocking.Abstractions.Repositories;
using Moq;
using TestHelpers.Attributes;
using Xunit;

namespace DistributedLocking.Abstractions.UnitTests
{
    public class DistributedLock_Should
    {
        [Theory]
        [AutoMoqData]
        public void Throw_When_TryingToCreateWithNullId(
            DistributedLockIdentifier identifier,
            IDistributedLockRepository distributedLockRepository)
        {
            Assert.Throws<ArgumentNullException>(() => new DistributedLock(identifier, null, distributedLockRepository));
        }
        
        [Theory]
        [AutoMoqData]
        public void Throw_When_TryingToCreateWithNullIdentifier(
            DistributedLockId id,
            IDistributedLockRepository distributedLockRepository)
        {
            Assert.Throws<ArgumentNullException>(() => new DistributedLock(null, id, distributedLockRepository));
        }
        
        [Theory]
        [AutoMoqData]
        public void Throw_When_TryingToCreateWithNullRepository(
            DistributedLockId id,
            DistributedLockIdentifier identifier)
        {
            Assert.Throws<ArgumentNullException>(() => new DistributedLock(identifier, id, null));
        }
        
        [Theory]
        [AutoMoqData]
        public void NotThrow_When_TryingToCreateWithoutNullValues(
            DistributedLockId id,
            DistributedLockIdentifier identifier,
            IDistributedLockRepository repository)
        {
            _ = new DistributedLock(identifier, id, repository);
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
                        distributedLock.Identifier,
                        distributedLock.Id,
                        CancellationToken.None),
                    Times.Once);
        }
    }
}