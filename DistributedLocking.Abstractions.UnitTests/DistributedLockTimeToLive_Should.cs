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
        public void ReturnTrue_When_ComparingDifferentObjectsWithSameValue(TimeSpan value)
        {
            DistributedLockTimeToLive id1 = value;
            DistributedLockTimeToLive id2 = value;
            
            Assert.Equal(id1, id2);
            Assert.True(id1 == id2);
            Assert.False(id1 != id2);
        }

        [Theory]
        [AutoMoqData]
        public void ReturnFalse_When_ComparingDifferentObjectsWithDifferentValue(TimeSpan value, TimeSpan otherValue)
        {
            DistributedLockTimeToLive id1 = value;
            DistributedLockTimeToLive id2 = otherValue;
            
            Assert.NotEqual(id1, id2);
            Assert.False(id1 == id2);
            Assert.True(id1 != id2);
        }

        [Theory]
        [AutoMoqData]
        public void ImplicitlyConvertFromTimeSpanToDistributedLockTimeToLive(TimeSpan value)
        {
            DistributedLockTimeToLive timeToLive = value;
            
            Assert.Equal(value, timeToLive.Value);
        }

        [Theory]
        [AutoMoqData]
        public void ImplicitlyConvertFromDistributedLockTimeToLiveToTimeSpan(DistributedLockTimeToLive timeToLive)
        {
            TimeSpan value = timeToLive;
            
            Assert.Equal(timeToLive.Value, value);
        }
    }
}