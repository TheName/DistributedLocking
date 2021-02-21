using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.Abstractions;
using DistributedLocking.Exceptions;
using DistributedLocking.Extensions;
using Moq;
using TestHelpers.Attributes;
using Xunit;

namespace Extensions.UnitTests
{
    public class DistributedLockProviderExtensions_Should
    {
        [Theory]
        [AutoMoqData]
        public async Task Throw_When_AcquiringFails(
            DistributedLockResourceId resourceId,
            DistributedLockTimeToLive timeToLive,
            Mock<IDistributedLockProvider> distributedLockProviderMock,
            CancellationToken cancellationToken)
        {
            var provider = distributedLockProviderMock.Object;
            distributedLockProviderMock
                .Setup(lockProvider => lockProvider.TryAcquireAsync(
                    resourceId,
                    timeToLive,
                    cancellationToken))
                .ReturnsAsync(null as IDistributedLock);

            var exception = await Assert.ThrowsAsync<CouldNotAcquireDistributedLockException>(() =>
                provider.AcquireAsync(resourceId, timeToLive, cancellationToken));
            
            Assert.Equal(resourceId, exception.ResourceId);
            Assert.Equal(timeToLive, exception.TimeToLive);
        }
        
        [Theory]
        [AutoMoqData]
        public async Task NotThrow_When_AcquiringSucceeds(
            DistributedLockResourceId resourceId,
            DistributedLockTimeToLive timeToLive,
            IDistributedLock distributedLock,
            Mock<IDistributedLockProvider> distributedLockProviderMock,
            CancellationToken cancellationToken)
        {
            var provider = distributedLockProviderMock.Object;
            distributedLockProviderMock
                .Setup(lockProvider => lockProvider.TryAcquireAsync(
                    resourceId,
                    timeToLive,
                    cancellationToken))
                .ReturnsAsync(distributedLock);

            var acquiredLock = await provider.AcquireAsync(resourceId, timeToLive, cancellationToken);
            
            Assert.Equal(distributedLock, acquiredLock);
        }
    }
}