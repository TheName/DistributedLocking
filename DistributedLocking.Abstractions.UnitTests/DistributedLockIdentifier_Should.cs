using System;
using TestHelpers.Attributes;
using Xunit;

namespace DistributedLocking.Abstractions.UnitTests
{
    public class DistributedLockIdentifier_Should
    {
        [Fact]
        public void Throw_When_TryingToCreateWithEmptyGuid()
        {
            Assert.Throws<ArgumentException>(() => new DistributedLockIdentifier(Guid.Empty));
        }

        [Theory]
        [AutoMoqData]
        public void NotThrow_When_TryingToCreateWithNonEmptyGuid(Guid identifier)
        {
            _ = new DistributedLockIdentifier(identifier);
        }

        [Theory]
        [AutoMoqData]
        public void ReturnTrue_When_ComparingDifferentObjectsWithSameValue(Guid value)
        {
            DistributedLockIdentifier id1 = value;
            DistributedLockIdentifier id2 = value;
            
            Assert.Equal(id1, id2);
            Assert.True(id1 == id2);
            Assert.False(id1 != id2);
        }

        [Theory]
        [AutoMoqData]
        public void ReturnFalse_When_ComparingDifferentObjectsWithDifferentValue(Guid value, Guid otherValue)
        {
            DistributedLockIdentifier id1 = value;
            DistributedLockIdentifier id2 = otherValue;
            
            Assert.NotEqual(id1, id2);
            Assert.False(id1 == id2);
            Assert.True(id1 != id2);
        }

        [Theory]
        [AutoMoqData]
        public void ImplicitlyConvertFromGuidToDistributedLockIdentifier(Guid value)
        {
            DistributedLockIdentifier identifier = value;
            
            Assert.Equal(value, identifier.Value);
        }

        [Theory]
        [AutoMoqData]
        public void ImplicitlyConvertFromDistributedLockIdentifierToGuid(DistributedLockIdentifier identifier)
        {
            Guid value = identifier;
            
            Assert.Equal(identifier.Value, value);
        }
    }
}