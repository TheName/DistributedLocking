using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Kernel;
using DistributedLocking.Abstractions;

namespace TestHelpers.AutoFixture
{
    public static class AutoFixtureFactory
    {
        public static IFixture Create()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization {ConfigureMembers = true});

            fixture.Register<ISpecimenBuilder, DistributedLockResourceId>(builder => (DistributedLockResourceId) builder.Create<string>());

            return fixture;
        }
    }
}