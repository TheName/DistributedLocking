using System;
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
            fixture.Register<ISpecimenBuilder, DistributedLockTimeToLive>(builder =>
                (DistributedLockTimeToLive) (TimeSpan.Zero + TimeSpan.FromMilliseconds(builder.Create<uint>())));

            return fixture;
        }
    }
}