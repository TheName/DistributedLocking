using System;
using AutoFixture.Xunit2;
using TestHelpers.Attributes;
using Xunit;

namespace DistributedLocking.Abstractions.UnitTests
{
    public class DistributedLockResourceId_Should
    {
        [Theory]
        [InlineAutoData("")]
        [InlineAutoData(" ")]
        [InlineAutoData("\n")]
        [InlineAutoData((string)null)]
        public void Throw_When_TryingToCreateWithNullOrWhitespaceValue(string resourceId)
        {
            Assert.Throws<ArgumentException>(() => (DistributedLockResourceId) resourceId);
        }

        [Theory]
        [AutoMoqData]
        public void NotThrow_When_TryingToCreateWithNonNullOrWhitespaceValue(string resourceId)
        {
            DistributedLockResourceId _ = resourceId;
        }

        [Theory]
        [AutoMoqData]
        public void ReturnTrue_When_ComparingDifferentObjectsWithSameValue(string value)
        {
            DistributedLockResourceId id1 = value;
            DistributedLockResourceId id2 = value;
            
            Assert.Equal(id1, id2);
            Assert.True(id1 == id2);
            Assert.False(id1 != id2);
        }

        [Theory]
        [AutoMoqData]
        public void ReturnFalse_When_ComparingDifferentObjectsWithDifferentValue(string value, string otherValue)
        {
            DistributedLockResourceId id1 = value;
            DistributedLockResourceId id2 = otherValue;
            
            Assert.NotEqual(id1, id2);
            Assert.False(id1 == id2);
            Assert.True(id1 != id2);
        }

        [Theory]
        [AutoMoqData]
        public void ImplicitlyConvertFromGuidToDistributedLockResourceId(Guid value)
        {
            DistributedLockResourceId resourceId = value;
            
            Assert.Equal(value.ToString(), resourceId.Value);
        }

        [Theory]
        [AutoMoqData]
        public void ImplicitlyConvertFromStringToDistributedLockResourceId(string value)
        {
            DistributedLockResourceId resourceId = value;
            
            Assert.Equal(value, resourceId.Value);
        }

        [Theory]
        [AutoMoqData]
        public void ImplicitlyConvertFromDistributedLockResourceIdToString(DistributedLockResourceId resourceId)
        {
            string value = resourceId;
            
            Assert.Equal(resourceId.Value, value);
        }

        [Theory]
        [AutoMoqData]
        public void ReturnValueToString_When_CallingToString(DistributedLockResourceId resourceId)
        {
            Assert.Equal(resourceId.Value, resourceId.ToString());
        }
    }
}