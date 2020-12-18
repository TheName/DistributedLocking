using System;
using TestHelpers.Attributes;
using TheName.DistributedLocking.Abstractions.Records;
using Xunit;

namespace DistributedLocking.Abstractions.UnitTests.Records
{
    public class LockId_Should
    {
        [Fact]
        public void Throw_When_TryingToCreateWithEmptyGuid()
        {
            Assert.Throws<ArgumentException>(() => new LockId(Guid.Empty));
        }

        [Theory]
        [AutoMoqData]
        public void NotThrow_When_TryingToCreateWithNonEmptyGuid(
            Guid lockId)
        {
            _ = new LockId(lockId);
        }
    }
}