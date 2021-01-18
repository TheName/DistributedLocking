using System;
using TestHelpers.Attributes;
using Xunit;

namespace DistributedLocking.Abstractions.UnitTests
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
        public void ReturnTrue_When_ComparingDifferentObjectsWithSameValue(Guid value)
        {
            DistributedLockId id1 = value;
            DistributedLockId id2 = value;
            
            Assert.Equal(id1, id2);
            Assert.True(id1 == id2);
            Assert.False(id1 != id2);
        }

        [Theory]
        [AutoMoqData]
        public void ReturnFalse_When_ComparingDifferentObjectsWithDifferentValue(Guid value, Guid otherValue)
        {
            DistributedLockId id1 = value;
            DistributedLockId id2 = otherValue;
            
            Assert.NotEqual(id1, id2);
            Assert.False(id1 == id2);
            Assert.True(id1 != id2);
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