using System;
using AutoFixture.Xunit2;
using TestHelpers.Attributes;
using Xunit;

namespace DistributedLocking.Abstractions.UnitTests
{
    public class DistributedLockIdentifier_Should
    {
        [Theory]
        [InlineAutoData("")]
        [InlineAutoData(" ")]
        [InlineAutoData("\n")]
        [InlineAutoData((string)null)]
        public void Throw_When_TryingToCreateWithNullOrWhitespaceValue(string identifier)
        {
            Assert.Throws<ArgumentException>(() => new DistributedLockIdentifier(identifier));
        }

        [Theory]
        [AutoMoqData]
        public void NotThrow_When_TryingToCreateWithNonNullOrWhitespaceValue(string identifier)
        {
            _ = new DistributedLockIdentifier(identifier);
        }

        [Theory]
        [AutoMoqData]
        public void ReturnTrue_When_ComparingDifferentObjectsWithSameValue(string value)
        {
            DistributedLockIdentifier id1 = value;
            DistributedLockIdentifier id2 = value;
            
            Assert.Equal(id1, id2);
            Assert.True(id1 == id2);
            Assert.False(id1 != id2);
        }

        [Theory]
        [AutoMoqData]
        public void ReturnFalse_When_ComparingDifferentObjectsWithDifferentValue(string value, string otherValue)
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
            
            Assert.Equal(value.ToString(), identifier.Value);
        }

        [Theory]
        [AutoMoqData]
        public void ImplicitlyConvertFromStringToDistributedLockIdentifier(string value)
        {
            DistributedLockIdentifier identifier = value;
            
            Assert.Equal(value, identifier.Value);
        }

        [Theory]
        [AutoMoqData]
        public void ImplicitlyConvertFromDistributedLockIdentifierToString(DistributedLockIdentifier identifier)
        {
            string value = identifier;
            
            Assert.Equal(identifier.Value, value);
        }

        [Theory]
        [AutoMoqData]
        public void ReturnValueToString_When_CallingToString(DistributedLockIdentifier identifier)
        {
            Assert.Equal(identifier.Value, identifier.ToString());
        }
    }
}