using System;
using DistributedLocking.Abstractions.Records;
using TestHelpers.Attributes;
using Xunit;

namespace DistributedLocking.Abstractions.UnitTests.Records
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