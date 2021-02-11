using System;
using TestHelpers.Attributes;
using Xunit;

namespace DistributedLocking.Abstractions.UnitTests
{
    public class DistributedLockTimeToLive_Should
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

        [Theory]
        [AutoMoqData]
        public void ReturnTrue_When_ComparingDifferentObjectsWithSameValue(uint milliseconds)
        {
            var positiveTimespan = TimeSpan.Zero + TimeSpan.FromMilliseconds(milliseconds);
            
            DistributedLockTimeToLive id1 = positiveTimespan;
            DistributedLockTimeToLive id2 = positiveTimespan;
            
            Assert.Equal(id1, id2);
            Assert.True(id1 == id2);
            Assert.False(id1 != id2);
        }

        [Theory]
        [AutoMoqData]
        public void ReturnFalse_When_ComparingDifferentObjectsWithDifferentValue(
            uint milliseconds,
            uint otherMilliseconds)
        {
            var positiveTimespan = TimeSpan.Zero + TimeSpan.FromMilliseconds(milliseconds);
            var otherPositiveTimespan = TimeSpan.Zero + TimeSpan.FromMilliseconds(otherMilliseconds);

            DistributedLockTimeToLive id1 = positiveTimespan;
            DistributedLockTimeToLive id2 = otherPositiveTimespan;
            
            Assert.NotEqual(id1, id2);
            Assert.False(id1 == id2);
            Assert.True(id1 != id2);
        }

        [Theory]
        [AutoMoqData]
        public void ImplicitlyConvertFromTimeSpanToDistributedLockTimeToLive(uint milliseconds)
        {
            var positiveTimespan = TimeSpan.Zero + TimeSpan.FromMilliseconds(milliseconds);
            DistributedLockTimeToLive timeToLive = positiveTimespan;
            
            Assert.Equal(positiveTimespan, timeToLive.Value);
        }

        [Theory]
        [AutoMoqData]
        public void ImplicitlyConvertFromDistributedLockTimeToLiveToTimeSpan(DistributedLockTimeToLive timeToLive)
        {
            TimeSpan value = timeToLive;
            
            Assert.Equal(timeToLive.Value, value);
        }

        [Theory]
        [AutoMoqData]
        public void ReturnValueToString_When_CallingToString(DistributedLockTimeToLive timeToLive)
        {
            Assert.Equal(timeToLive.Value.ToString(), timeToLive.ToString());
        }
    }
}