using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using DistributedLocking.Abstractions.Factories;
using DistributedLocking.Abstractions.Managers;
using DistributedLocking.Managers;
using Moq;
using TestHelpers.Attributes;
using Xunit;

namespace DistributedLocking.UnitTests.Managers
{
    public class DistributedLockRepositoryManagerProxy_Should
    {
        [Fact]
        public void Throw_When_Creating_And_FactoryIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new DistributedLockRepositoryManagerProxy(null));
        }
        
        [Theory]
        [AutoMoqData]
        public void NotThrow_When_Creating_And_FactoryIsNotNull(IDistributedLockRepositoryManagerFactory repositoryManagerFactory)
        {
            _ = new DistributedLockRepositoryManagerProxy(repositoryManagerFactory);
        }

        [Theory]
        [AutoMoqWithInlineData(true)]
        [AutoMoqWithInlineData(false)]
        public async Task ReturnResultFromCreatedRepositoryManager_When_CallingRepositoryExists(
            bool repositoryExists,
            [Frozen] IDistributedLockRepositoryManagerFactory repositoryManagerFactory,
            DistributedLockRepositoryManagerProxy repositoryManagerProxy)
        {
            var repositoryManagerMock = new Mock<IDistributedLockRepositoryManager>();
            repositoryManagerMock
                .Setup(manager => manager.RepositoryExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(repositoryExists);

            Mock.Get(repositoryManagerFactory)
                .Setup(factory => factory.Create())
                .Returns(repositoryManagerMock.Object);

            var result = await repositoryManagerProxy.RepositoryExistsAsync(CancellationToken.None);
            
            Assert.Equal(repositoryExists, result);
        }

        [Theory]
        [AutoMoqData]
        public async Task CallCreatedRepositoryManager_When_CallingCreateIfNotExists(
            [Frozen] IDistributedLockRepositoryManagerFactory repositoryManagerFactory,
            DistributedLockRepositoryManagerProxy repositoryManagerProxy)
        {
            var repositoryManagerMock = new Mock<IDistributedLockRepositoryManager>();
            Mock.Get(repositoryManagerFactory)
                .Setup(factory => factory.Create())
                .Returns(repositoryManagerMock.Object);

            await repositoryManagerProxy.CreateIfNotExistsAsync(CancellationToken.None);

            repositoryManagerMock
                .Verify(
                    manager => manager.CreateIfNotExistsAsync(It.IsAny<CancellationToken>()),
                    Times.Once);
            
            repositoryManagerMock.VerifyNoOtherCalls();
        }
    }
}