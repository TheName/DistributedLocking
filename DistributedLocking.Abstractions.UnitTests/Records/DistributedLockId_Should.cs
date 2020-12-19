using System;
using TestHelpers.Attributes;
using TheName.DistributedLocking.Abstractions.Records;
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
        public void NotThrow_When_TryingToCreateWithNonEmptyGuid(
            Guid lockId)
        {
            _ = new DistributedLockId(lockId);
        }

        [Theory]
        [AutoMoqData]
        public void ImplicitlyConvertFromGuidToDistributedLockId(Guid value)
        {
            DistributedLockId lockId = value;
            
            Assert.Equal(value, lockId.Value);
        }

        [Theory]
        [AutoMoqData]
        public void ImplicitlyConvertFromDistributedLockIdToGuid(DistributedLockId lockId)
        {
            Guid value = lockId;
            
            Assert.Equal(lockId.Value, value);
        }
    }
}