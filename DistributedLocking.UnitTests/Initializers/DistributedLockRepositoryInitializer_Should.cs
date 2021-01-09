using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using DistributedLocking.Abstractions.Managers;
using DistributedLocking.Initializers;
using Moq;
using TestHelpers.Attributes;
using Xunit;

namespace DistributedLocking.UnitTests.Initializers
{
    public class DistributedLockRepositoryInitializer_Should
    {
        [Theory]
        [AutoMoqData]
        public async Task NotCreateRepository_When_Initializing_And_RepositoryExists(
            [Frozen] Mock<IDistributedLockRepositoryManager> repositoryManager,
            DistributedLockRepositoryInitializer initializer)
        {
            repositoryManager
                .Setup(manager => manager.RepositoryExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await initializer.InitializeAsync(CancellationToken.None);

            repositoryManager
                .Verify(
                    manager => manager.CreateIfNotExistsAsync(It.IsAny<CancellationToken>()),
                    Times.Never);
        }
        
        [Theory]
        [AutoMoqData]
        public async Task CreateRepository_When_Initializing_And_RepositoryDoesNotExist(
            [Frozen] Mock<IDistributedLockRepositoryManager> repositoryManager,
            DistributedLockRepositoryInitializer initializer)
        {
            repositoryManager
                .Setup(manager => manager.RepositoryExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            await initializer.InitializeAsync(CancellationToken.None);

            repositoryManager
                .Verify(
                    manager => manager.CreateIfNotExistsAsync(It.IsAny<CancellationToken>()),
                    Times.Once);
        }
    }
}