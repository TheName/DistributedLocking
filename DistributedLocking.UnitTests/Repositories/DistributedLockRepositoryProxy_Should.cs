using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Moq;
using TestHelpers.Attributes;
using TheName.DistributedLocking.Abstractions.Factories;
using TheName.DistributedLocking.Abstractions.Records;
using TheName.DistributedLocking.Abstractions.Repositories;
using TheName.DistributedLocking.Repositories;
using Xunit;

namespace DistributedLocking.UnitTests.Repositories
{
    public class DistributedLockRepositoryProxy_Should
    {
        [Fact]
        public void Throw_When_Creating_And_FactoryIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new DistributedLockRepositoryProxy(null));
        }
        
        [Theory]
        [AutoMoqData]
        public void NotThrow_When_Creating_And_FactoryIsNotNull(IDistributedLockRepositoryFactory repositoryFactory)
        {
            _ = new DistributedLockRepositoryProxy(repositoryFactory);
        }

        [Theory]
        [AutoMoqWithInlineData(true)]
        [AutoMoqWithInlineData(false)]
        public async Task ReturnResultFromCreatedRepository_When_CallingTryAcquireAsync(
            bool success,
            DistributedLockId lockId,
            DistributedLockIdentifier lockIdentifier,
            DistributedLockTimeToLive lockTimeToLive,
            [Frozen] IDistributedLockRepositoryFactory repositoryFactory,
            DistributedLockRepositoryProxy repositoryProxy)
        {
            var repositoryMock = new Mock<IDistributedLockRepository>();
            repositoryMock
                .Setup(repository => repository.TryAcquireAsync(lockIdentifier, lockTimeToLive, It.IsAny<CancellationToken>()))
                .ReturnsAsync((success, lockId));

            Mock.Get(repositoryFactory)
                .Setup(factory => factory.Create())
                .Returns(repositoryMock.Object);

            var result = await repositoryProxy.TryAcquireAsync(lockIdentifier, lockTimeToLive, CancellationToken.None);
            
            Assert.Equal(success, result.Success);
            Assert.Equal(lockId, result.AcquiredLockId);
        }

        [Theory]
        [AutoMoqWithInlineData(true)]
        [AutoMoqWithInlineData(false)]
        public async Task ReturnResultFromCreatedRepository_When_CallingTryReleaseAsync(
            bool success,
            DistributedLockId lockId,
            [Frozen] IDistributedLockRepositoryFactory repositoryFactory,
            DistributedLockRepositoryProxy repositoryProxy)
        {
            var repositoryMock = new Mock<IDistributedLockRepository>();
            repositoryMock
                .Setup(repository => repository.TryReleaseAsync(lockId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(success);

            Mock.Get(repositoryFactory)
                .Setup(factory => factory.Create())
                .Returns(repositoryMock.Object);

            var result = await repositoryProxy.TryReleaseAsync(lockId, CancellationToken.None);
            
            Assert.Equal(success, result);
        }

        [Theory]
        [AutoMoqWithInlineData(true)]
        [AutoMoqWithInlineData(false)]
        public async Task ReturnResultFromCreatedRepository_When_CallingTryExtendAsync(
            bool success,
            DistributedLockIdentifier lockIdentifier,
            DistributedLockId lockId,
            DistributedLockTimeToLive timeToLive,
            [Frozen] IDistributedLockRepositoryFactory repositoryFactory,
            DistributedLockRepositoryProxy repositoryProxy)
        {
            var repositoryMock = new Mock<IDistributedLockRepository>();
            repositoryMock
                .Setup(repository => repository.TryExtendAsync(lockIdentifier, lockId, timeToLive, It.IsAny<CancellationToken>()))
                .ReturnsAsync(success);

            Mock.Get(repositoryFactory)
                .Setup(factory => factory.Create())
                .Returns(repositoryMock.Object);

            var result = await repositoryProxy.TryExtendAsync(lockIdentifier, lockId, timeToLive, CancellationToken.None);
            
            Assert.Equal(success, result);
        }
    }
}