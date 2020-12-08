using AutoFixture.Xunit2;
using DistributedLocking.UnitTests.AutoFixture;

namespace DistributedLocking.UnitTests.Attributes
{
    public class AutoMoqDataAttribute : AutoDataAttribute
    {
        public AutoMoqDataAttribute() : base(AutoFixtureFactory.Create)
        {
        }
    }
}