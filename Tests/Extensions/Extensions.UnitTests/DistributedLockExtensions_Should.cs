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
    public class DistributedLockExtensions_Should
    {
        [Theory]
        [AutoMoqData]
        public async Task Throw_When_ExtendingFails(
            DistributedLockTimeToLive timeToLive,
            Mock<IDistributedLock> distributedLockMock,
            CancellationToken cancellationToken)
        {
            var distributedLock = distributedLockMock.Object;
            distributedLockMock
                .Setup(@lock => @lock.TryExtendAsync(timeToLive, cancellationToken))
                .ReturnsAsync(false);

            var exception = await Assert.ThrowsAsync<CouldNotExtendDistributedLockException>(() =>
                distributedLock.ExtendAsync(timeToLive, cancellationToken));
            
            Assert.Equal(distributedLock.ResourceId, exception.ResourceId);
            Assert.Equal(distributedLock.Id, exception.Id);
            Assert.Equal(timeToLive, exception.TimeToLive);
        }
        
        [Theory]
        [AutoMoqData]
        public async Task NotThrow_When_ExtendingSucceeds(
            Mock<IDistributedLock> distributedLockMock,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken)
        {
            var distributedLock = distributedLockMock.Object;
            distributedLockMock
                .Setup(@lock => @lock.TryExtendAsync(timeToLive, cancellationToken))
                .ReturnsAsync(true);

            await distributedLock.ExtendAsync(timeToLive, cancellationToken);
        }
        
        [Theory]
        [AutoMoqData]
        public async Task Throw_When_ReleasingFails(
            Mock<IDistributedLock> distributedLockMock,
            CancellationToken cancellationToken)
        {
            var distributedLock = distributedLockMock.Object;
            distributedLockMock
                .Setup(@lock => @lock.TryReleaseAsync(cancellationToken))
                .ReturnsAsync(false);

            var exception = await Assert.ThrowsAsync<CouldNotReleaseDistributedLockException>(() =>
                distributedLock.ReleaseAsync(cancellationToken));
            
            Assert.Equal(distributedLock.ResourceId, exception.ResourceId);
            Assert.Equal(distributedLock.Id, exception.Id);
        }
        
        [Theory]
        [AutoMoqData]
        public async Task NotThrow_When_ReleasingSucceeds(
            Mock<IDistributedLock> distributedLockMock,
            CancellationToken cancellationToken)
        {
            var distributedLock = distributedLockMock.Object;
            distributedLockMock
                .Setup(@lock => @lock.TryReleaseAsync(cancellationToken))
                .ReturnsAsync(true);

            await distributedLock.ReleaseAsync(cancellationToken);
        }
    }
}