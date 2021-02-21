using System;
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
    public class DistributedLockFacadeExtensions_Should
    {
        [Theory]
        [AutoMoqData]
        public async Task Throw_When_Acquiring_And_FacadeIsNull(
            DistributedLockResourceId resourceId,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken)
        {
            IDistributedLockFacade facade = null;

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                facade.AcquireAsync(resourceId, timeToLive, cancellationToken));
        }
        
        [Theory]
        [AutoMoqData]
        public async Task Throw_When_Acquiring_And_FacadeFails(
            DistributedLockResourceId resourceId,
            DistributedLockTimeToLive timeToLive,
            Mock<IDistributedLockFacade> facadeMock,
            CancellationToken cancellationToken)
        {
            facadeMock
                .Setup(x => x.TryAcquireAsync(resourceId, timeToLive, cancellationToken))
                .ReturnsAsync((false, null));

            var facade = facadeMock.Object;

            var exception = await Assert.ThrowsAsync<CouldNotAcquireDistributedLockException>(() =>
                facade.AcquireAsync(resourceId, timeToLive, cancellationToken));
            
            Assert.Equal(resourceId, exception.ResourceId);
            Assert.Equal(timeToLive, exception.TimeToLive);
        }

        [Theory]
        [AutoMoqData]
        public async Task ReturnDistributedLock_When_Acquiring_And_FacadeSucceeds(
            IDistributedLock distributedLock,
            DistributedLockResourceId resourceId,
            DistributedLockTimeToLive timeToLive,
            Mock<IDistributedLockFacade> facadeMock,
            CancellationToken cancellationToken)
        {
            facadeMock
                .Setup(x => x.TryAcquireAsync(resourceId, timeToLive, cancellationToken))
                .ReturnsAsync((true, distributedLock));

            var facade = facadeMock.Object;

            var result = await facade.AcquireAsync(resourceId, timeToLive, cancellationToken);
            
            Assert.Equal(distributedLock, result);
        }
        
        [Theory]
        [AutoMoqData]
        public async Task Throw_When_Extending_And_FacadeIsNull(
            DistributedLockResourceId resourceId,
            DistributedLockId id,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken)
        {
            IDistributedLockFacade facade = null;

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                facade.ExtendAsync(resourceId, id, timeToLive, cancellationToken));
        }
        
        [Theory]
        [AutoMoqData]
        public async Task Throw_When_Extending_And_FacadeFails(
            DistributedLockResourceId resourceId,
            DistributedLockId id,
            DistributedLockTimeToLive timeToLive,
            Mock<IDistributedLockFacade> facadeMock,
            CancellationToken cancellationToken)
        {
            facadeMock
                .Setup(x => x.TryExtendAsync(resourceId, id, timeToLive, cancellationToken))
                .ReturnsAsync(false);

            var facade = facadeMock.Object;

            var exception = await Assert.ThrowsAsync<CouldNotExtendDistributedLockException>(() =>
                facade.ExtendAsync(resourceId, id, timeToLive, cancellationToken));
            
            Assert.Equal(resourceId, exception.ResourceId);
            Assert.Equal(id, exception.Id);
            Assert.Equal(timeToLive, exception.TimeToLive);
        }
        
        [Theory]
        [AutoMoqData]
        public async Task NotThrow_When_Extending_And_FacadeSucceeds(
            DistributedLockResourceId resourceId,
            DistributedLockId id,
            DistributedLockTimeToLive timeToLive,
            Mock<IDistributedLockFacade> facadeMock,
            CancellationToken cancellationToken)
        {
            facadeMock
                .Setup(x => x.TryExtendAsync(resourceId, id, timeToLive, cancellationToken))
                .ReturnsAsync(true);

            var facade = facadeMock.Object;

            await facade.ExtendAsync(resourceId, id, timeToLive, cancellationToken);
        }
        
        [Theory]
        [AutoMoqData]
        public async Task Throw_When_Releasing_And_FacadeIsNull(
            DistributedLockResourceId resourceId,
            DistributedLockId id,
            CancellationToken cancellationToken)
        {
            IDistributedLockFacade facade = null;

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                facade.ReleaseAsync(resourceId, id, cancellationToken));
        }
        
        [Theory]
        [AutoMoqData]
        public async Task Throw_When_Releasing_And_FacadeFails(
            DistributedLockResourceId resourceId,
            DistributedLockId id,
            Mock<IDistributedLockFacade> facadeMock,
            CancellationToken cancellationToken)
        {
            facadeMock
                .Setup(x => x.TryReleaseAsync(resourceId, id, cancellationToken))
                .ReturnsAsync(false);

            var facade = facadeMock.Object;

            var exception = await Assert.ThrowsAsync<CouldNotReleaseDistributedLockException>(() =>
                facade.ReleaseAsync(resourceId, id, cancellationToken));
            
            Assert.Equal(resourceId, exception.ResourceId);
            Assert.Equal(id, exception.Id);
        }
        
        [Theory]
        [AutoMoqData]
        public async Task NotThrow_When_Releasing_And_FacadeSucceeds(
            DistributedLockResourceId resourceId,
            DistributedLockId id,
            Mock<IDistributedLockFacade> facadeMock,
            CancellationToken cancellationToken)
        {
            facadeMock
                .Setup(x => x.TryReleaseAsync(resourceId, id, cancellationToken))
                .ReturnsAsync(true);

            var facade = facadeMock.Object;

            await facade.ReleaseAsync(resourceId, id, cancellationToken);
        }
    }
}