using AutoFixture;
using AutoFixture.AutoMoq;

namespace TestHelpers.AutoFixture
{
    public static class AutoFixtureFactory
    {
        public static IFixture Create() =>
            new Fixture().Customize(new AutoMoqCustomization {ConfigureMembers = true});
    }
}