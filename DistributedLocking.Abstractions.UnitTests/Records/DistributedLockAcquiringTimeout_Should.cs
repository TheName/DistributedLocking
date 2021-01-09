using System;
using DistributedLocking.Abstractions.Records;
using TestHelpers.Attributes;
using Xunit;

namespace DistributedLocking.Abstractions.UnitTests.Records
{
    public class DistributedLockAcquiringTimeout_Should
    {
        [Fact]
        public void Throw_When_TryingToCreateWithZeroTimespan()
        {
            Assert.Throws<ArgumentException>(() => new DistributedLockAcquiringTimeout(TimeSpan.Zero));
        }
        
        [Theory]
        [AutoMoqData]
        public void Throw_When_TryingToCreateWithNegativeTimespan(uint milliseconds)
        {
            Assert.Throws<ArgumentException>(() =>
                new DistributedLockAcquiringTimeout(TimeSpan.Zero - TimeSpan.FromMilliseconds(milliseconds)));
        }
        
        [Theory]
        [AutoMoqData]
        public void NotThrow_When_TryingToCreateWithPositiveTimespan(uint milliseconds)
        {
            _ = new DistributedLockAcquiringTimeout(TimeSpan.Zero + TimeSpan.FromMilliseconds(milliseconds));
        }

        [Theory]
        [AutoMoqData]
        public void ImplicitlyConvertFromTimeSpanToDistributedLockAcquiringTimeout(TimeSpan value)
        {
            DistributedLockAcquiringTimeout timeout = value;
            
            Assert.Equal(value, timeout.Value);
        }

        [Theory]
        [AutoMoqData]
        public void ImplicitlyConvertFromDistributedLockAcquiringTimeoutToTimeSpan(DistributedLockAcquiringTimeout timeout)
        {
            TimeSpan value = timeout;
            
            Assert.Equal(timeout.Value, value);
        }
    }
}