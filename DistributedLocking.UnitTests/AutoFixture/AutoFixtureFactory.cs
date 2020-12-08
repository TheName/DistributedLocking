using AutoFixture;
using AutoFixture.AutoMoq;

namespace DistributedLocking.UnitTests.AutoFixture
{
    public static class AutoFixtureFactory
    {
        public static IFixture Create() =>
            new Fixture().Customize(new AutoMoqCustomization {ConfigureMembers = true});
    }
}