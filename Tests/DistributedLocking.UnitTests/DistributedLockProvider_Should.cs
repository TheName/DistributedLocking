using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using DistributedLocking.Abstractions;
using DistributedLocking.Abstractions.Repositories;
using Moq;
using TestHelpers.Attributes;
using Xunit;

namespace DistributedLocking.UnitTests
{
    public class DistributedLockProvider_Should
    {
        [Fact]
        public void Throw_When_CreatingWithNullRepository()
        {
            Assert.Throws<ArgumentNullException>(() => new DistributedLockProvider(null));
        }

        [Theory]
        [AutoMoqData]
        public void NotThrow_When_CreatingWithNotNullRepository(IDistributedLocksRepository repository)
        {
            _ = new DistributedLockProvider(repository);
        }
        
        [Theory]
        [AutoMoqData]
        public async Task Throw_When_TryingToAcquire_WithNullResourceId(
            DistributedLockTimeToLive timeToLive,
            DistributedLockProvider provider)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => provider.TryAcquireAsync(
                null,
                timeToLive,
                CancellationToken.None));
        }
        
        [Theory]
        [AutoMoqData]
        public async Task Throw_When_TryingToAcquire_WithNullTimeToLive(
            DistributedLockResourceId resourceId,
            DistributedLockProvider provider)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => provider.TryAcquireAsync(
                resourceId,
                null,
                CancellationToken.None));
        }
        
        [Theory]
        [AutoMoqData]
        public async Task NotThrow_When_TryingToAcquire_WithoutNulls(
            DistributedLockResourceId resourceId,
            DistributedLockTimeToLive timeToLive,
            DistributedLockProvider provider)
        {
            await provider.TryAcquireAsync(
                resourceId,
                timeToLive,
                CancellationToken.None);
        }
        
        [Theory]
        [AutoMoqData]
        public async Task ReturnDistributedLock_When_TryingToAcquire_And_RepositorySucceeds(
            DistributedLockResourceId resourceId,
            DistributedLockTimeToLive timeToLive,
            [Frozen] Mock<IDistributedLocksRepository> repositoryMock,
            DistributedLockProvider provider)
        {
            repositoryMock
                .Setup(repository => repository.TryInsert(
                    resourceId,
                    It.IsAny<DistributedLockId>(),
                    timeToLive,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            
            var result = await provider.TryAcquireAsync(
                resourceId,
                timeToLive,
                CancellationToken.None);
            
            Assert.NotNull(result);
        }
        
        [Theory]
        [AutoMoqData]
        public async Task ReturnNull_When_TryingToAcquire_And_RepositoryFails(
            DistributedLockResourceId resourceId,
            DistributedLockTimeToLive timeToLive,
            [Frozen] Mock<IDistributedLocksRepository> repositoryMock,
            DistributedLockProvider provider)
        {
            repositoryMock
                .Setup(repository => repository.TryInsert(
                    resourceId,
                    It.IsAny<DistributedLockId>(),
                    timeToLive,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            
            var result = await provider.TryAcquireAsync(
                resourceId,
                timeToLive,
                CancellationToken.None);
            
            Assert.Null(result);
        }
    }
}