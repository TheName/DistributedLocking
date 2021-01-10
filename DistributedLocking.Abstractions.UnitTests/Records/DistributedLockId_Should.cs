using System;
using TestHelpers.Attributes;
using Xunit;

namespace DistributedLocking.Abstractions.UnitTests.Records
{
    public class DistributedLockId_Should
    {
        [Fact]
        public void Throw_When_TryingToCreateWithEmptyGuid()
        {
            Assert.Throws<ArgumentException>(() => new DistributedLockId(Guid.Empty));
        }

        [Theory]
        [AutoMoqData]
        public void NotThrow_When_TryingToCreateWithNonEmptyGuid(Guid id)
        {
            _ = new DistributedLockId(id);
        }

        [Theory]
        [AutoMoqData]
        public void ImplicitlyConvertFromGuidToDistributedLockId(Guid value)
        {
            DistributedLockId id = value;
            
            Assert.Equal(value, id.Value);
        }

        [Theory]
        [AutoMoqData]
        public void ImplicitlyConvertFromDistributedLockIdToGuid(DistributedLockId id)
        {
            Guid value = id;
            
            Assert.Equal(id.Value, value);
        }
    }
}