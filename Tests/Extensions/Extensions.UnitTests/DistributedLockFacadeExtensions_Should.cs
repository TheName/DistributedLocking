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
            DistributedLockIdentifier identifier,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken)
        {
            IDistributedLockFacade facade = null;

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                facade.AcquireAsync(identifier, timeToLive, cancellationToken));
        }
        
        [Theory]
        [AutoMoqData]
        public async Task Throw_When_Acquiring_And_FacadeFails(
            DistributedLockIdentifier identifier,
            DistributedLockTimeToLive timeToLive,
            Mock<IDistributedLockFacade> facadeMock,
            CancellationToken cancellationToken)
        {
            facadeMock
                .Setup(x => x.TryAcquireAsync(identifier, timeToLive, cancellationToken))
                .ReturnsAsync((false, null));

            var facade = facadeMock.Object;

            var exception = await Assert.ThrowsAsync<CouldNotAcquireDistributedLockException>(() =>
                facade.AcquireAsync(identifier, timeToLive, cancellationToken));
            
            Assert.Equal(identifier, exception.Identifier);
            Assert.Equal(timeToLive, exception.TimeToLive);
        }

        [Theory]
        [AutoMoqData]
        public async Task ReturnDistributedLock_When_Acquiring_And_FacadeSucceeds(
            IDistributedLock distributedLock,
            DistributedLockIdentifier identifier,
            DistributedLockTimeToLive timeToLive,
            Mock<IDistributedLockFacade> facadeMock,
            CancellationToken cancellationToken)
        {
            facadeMock
                .Setup(x => x.TryAcquireAsync(identifier, timeToLive, cancellationToken))
                .ReturnsAsync((true, distributedLock));

            var facade = facadeMock.Object;

            var result = await facade.AcquireAsync(identifier, timeToLive, cancellationToken);
            
            Assert.Equal(distributedLock, result);
        }
        
        [Theory]
        [AutoMoqData]
        public async Task Throw_When_Extending_And_FacadeIsNull(
            DistributedLockIdentifier identifier,
            DistributedLockId id,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken)
        {
            IDistributedLockFacade facade = null;

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                facade.ExtendAsync(identifier, id, timeToLive, cancellationToken));
        }
        
        [Theory]
        [AutoMoqData]
        public async Task Throw_When_Extending_And_FacadeFails(
            DistributedLockIdentifier identifier,
            DistributedLockId id,
            DistributedLockTimeToLive timeToLive,
            Mock<IDistributedLockFacade> facadeMock,
            CancellationToken cancellationToken)
        {
            facadeMock
                .Setup(x => x.TryExtendAsync(identifier, id, timeToLive, cancellationToken))
                .ReturnsAsync(false);

            var facade = facadeMock.Object;

            var exception = await Assert.ThrowsAsync<CouldNotExtendDistributedLockException>(() =>
                facade.ExtendAsync(identifier, id, timeToLive, cancellationToken));
            
            Assert.Equal(identifier, exception.Identifier);
            Assert.Equal(id, exception.Id);
            Assert.Equal(timeToLive, exception.TimeToLive);
        }
        
        [Theory]
        [AutoMoqData]
        public async Task NotThrow_When_Extending_And_FacadeSucceeds(
            DistributedLockIdentifier identifier,
            DistributedLockId id,
            DistributedLockTimeToLive timeToLive,
            Mock<IDistributedLockFacade> facadeMock,
            CancellationToken cancellationToken)
        {
            facadeMock
                .Setup(x => x.TryExtendAsync(identifier, id, timeToLive, cancellationToken))
                .ReturnsAsync(true);

            var facade = facadeMock.Object;

            await facade.ExtendAsync(identifier, id, timeToLive, cancellationToken);
        }
        
        [Theory]
        [AutoMoqData]
        public async Task Throw_When_Releasing_And_FacadeIsNull(
            DistributedLockIdentifier identifier,
            DistributedLockId id,
            CancellationToken cancellationToken)
        {
            IDistributedLockFacade facade = null;

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                facade.ReleaseAsync(identifier, id, cancellationToken));
        }
        
        [Theory]
        [AutoMoqData]
        public async Task Throw_When_Releasing_And_FacadeFails(
            DistributedLockIdentifier identifier,
            DistributedLockId id,
            Mock<IDistributedLockFacade> facadeMock,
            CancellationToken cancellationToken)
        {
            facadeMock
                .Setup(x => x.TryReleaseAsync(identifier, id, cancellationToken))
                .ReturnsAsync(false);

            var facade = facadeMock.Object;

            var exception = await Assert.ThrowsAsync<CouldNotReleaseDistributedLockException>(() =>
                facade.ReleaseAsync(identifier, id, cancellationToken));
            
            Assert.Equal(identifier, exception.Identifier);
            Assert.Equal(id, exception.Id);
        }
        
        [Theory]
        [AutoMoqData]
        public async Task NotThrow_When_Releasing_And_FacadeSucceeds(
            DistributedLockIdentifier identifier,
            DistributedLockId id,
            Mock<IDistributedLockFacade> facadeMock,
            CancellationToken cancellationToken)
        {
            facadeMock
                .Setup(x => x.TryReleaseAsync(identifier, id, cancellationToken))
                .ReturnsAsync(true);

            var facade = facadeMock.Object;

            await facade.ReleaseAsync(identifier, id, cancellationToken);
        }
    }
}