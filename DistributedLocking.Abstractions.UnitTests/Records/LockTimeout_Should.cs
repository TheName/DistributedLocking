using System;
using TestHelpers.Attributes;
using TheName.DistributedLocking.Abstractions.Records;
using Xunit;

namespace DistributedLocking.UnitTests.Abstractions.Records
{
    public class LockTimeout_Should
    {
        [Fact]
        public void Throw_When_TryingToCreateWithZeroTimespan()
        {
            Assert.Throws<ArgumentException>(() => new DistributedLockTimeToLive(TimeSpan.Zero));
        }
        
        [Theory]
        [AutoMoqData]
        public void Throw_When_TryingToCreateWithNegativeTimespan(uint milliseconds)
        {
            Assert.Throws<ArgumentException>(() =>
                new DistributedLockTimeToLive(TimeSpan.Zero - TimeSpan.FromMilliseconds(milliseconds)));
        }
        
        [Theory]
        [AutoMoqData]
        public void NotThrow_When_TryingToCreateWithPositiveTimespan(uint milliseconds)
        {
            _ = new DistributedLockTimeToLive(TimeSpan.Zero + TimeSpan.FromMilliseconds(milliseconds));
        }
    }
}