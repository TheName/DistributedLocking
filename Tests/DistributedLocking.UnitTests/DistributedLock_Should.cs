using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using DistributedLocking.Abstractions;
using Moq;
using TestHelpers.Attributes;
using Xunit;

namespace DistributedLocking.UnitTests
{
    public class DistributedLock_Should
    {
        [Theory]
        [AutoMoqData]
        public void Throw_When_TryingToCreateWithNullResourceId(
            DistributedLockId id,
            IDistributedLockFacade distributedLockFacade)
        {
            Assert.Throws<ArgumentNullException>(() => new DistributedLock(null, id, distributedLockFacade));
        }

        [Theory]
        [AutoMoqData]
        public void Throw_When_TryingToCreateWithNullId(
            DistributedLockResourceId resourceId,
            IDistributedLockFacade distributedLockFacade)
        {
            Assert.Throws<ArgumentNullException>(() => new DistributedLock(resourceId, null, distributedLockFacade));
        }
        
        [Theory]
        [AutoMoqData]
        public void Throw_When_TryingToCreateWithNullFacade(
            DistributedLockId id,
            DistributedLockResourceId resourceId)
        {
            Assert.Throws<ArgumentNullException>(() => new DistributedLock(resourceId, id, null));
        }
        
        [Theory]
        [AutoMoqData]
        public void NotThrow_When_TryingToCreateWithoutNullValues(
            DistributedLockId id,
            DistributedLockResourceId resourceId,
            IDistributedLockFacade distributedLockFacade)
        {
            _ = new DistributedLock(resourceId, id, distributedLockFacade);
        }

        [Theory]
        [AutoMoqWithInlineData(true)]
        [AutoMoqWithInlineData(false)]
        public async Task TryToExtendLock(
            bool expectedResult,
            DistributedLockTimeToLive timeToLive,
            [Frozen] Mock<IDistributedLockFacade> distributedLockFacadeMock,
            DistributedLock distributedLock,
            CancellationToken cancellationToken)
        {
            distributedLockFacadeMock
                .Setup(facade => facade.TryExtendAsync(
                    distributedLock.ResourceId,
                    distributedLock.Id,
                    timeToLive,
                    cancellationToken))
                .ReturnsAsync(expectedResult);
            
            var result = await distributedLock.TryExtendAsync(timeToLive, cancellationToken);

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [AutoMoqData]
        public async Task TryToReleaseLock_When_Disposing(
            [Frozen] Mock<IDistributedLockFacade> distributedLockFacadeMock,
            DistributedLock distributedLock)
        {
            await distributedLock.DisposeAsync();

            distributedLockFacadeMock
                .Verify(
                    repository => repository.TryReleaseAsync(
                        distributedLock.ResourceId,
                        distributedLock.Id,
                        CancellationToken.None),
                    Times.Once);
        }
    }
}