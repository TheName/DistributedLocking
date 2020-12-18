using System;
using TestHelpers.Attributes;
using TheName.DistributedLocking.Abstractions.Records;
using Xunit;

namespace DistributedLocking.UnitTests.Abstractions.Records
{
    public class LockIdentifier_Should
    {
        [Fact]
        public void Throw_When_TryingToCreateWithEmptyGuid()
        {
            Assert.Throws<ArgumentException>(() => new LockIdentifier(Guid.Empty));
        }

        [Theory]
        [AutoMoqData]
        public void NotThrow_When_TryingToCreateWithNonEmptyGuid(
            Guid lockId)
        {
            _ = new LockIdentifier(lockId);
        }
    }
}