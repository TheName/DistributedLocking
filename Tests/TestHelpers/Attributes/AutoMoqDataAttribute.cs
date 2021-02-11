using AutoFixture.Xunit2;
using TestHelpers.AutoFixture;

namespace TestHelpers.Attributes
{
    public class AutoMoqDataAttribute : AutoDataAttribute
    {
        public AutoMoqDataAttribute() : base(AutoFixtureFactory.Create)
        {
        }
    }
}