using System;
using DistributedLocking.Abstractions.Records;
using TestHelpers.Attributes;
using Xunit;

namespace DistributedLocking.Abstractions.UnitTests.Records
{
    public class DistributedLockAcquiringDelayBetweenRetries_Should
    {
        [Fact]
        public void NotThrow_When_TryingToCreateWithZeroTimespan()
        {
            _ = new DistributedLockAcquiringDelayBetweenRetries(TimeSpan.Zero);
        }
        
        [Theory]
        [AutoMoqData]
        public void Throw_When_TryingToCreateWithNegativeTimespan(uint milliseconds)
        {
            Assert.Throws<ArgumentException>(() =>
                new DistributedLockAcquiringDelayBetweenRetries(TimeSpan.Zero - TimeSpan.FromMilliseconds(milliseconds)));
        }
        
        [Theory]
        [AutoMoqData]
        public void NotThrow_When_TryingToCreateWithPositiveTimespan(uint milliseconds)
        {
            _ = new DistributedLockAcquiringDelayBetweenRetries(TimeSpan.Zero + TimeSpan.FromMilliseconds(milliseconds));
        }

        [Theory]
        [AutoMoqData]
        public void ImplicitlyConvertFromTimeSpanToDistributedLockAcquiringDelay(TimeSpan value)
        {
            DistributedLockAcquiringDelayBetweenRetries delay = value;
            
            Assert.Equal(value, delay.Value);
        }

        [Theory]
        [AutoMoqData]
        public void ImplicitlyConvertFromDistributedLockAcquiringDelayToTimeSpan(DistributedLockAcquiringDelayBetweenRetries delay)
        {
            TimeSpan value = delay;
            
            Assert.Equal(delay.Value, value);
        }
    }
}