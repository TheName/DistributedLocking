using System;
using DistributedLocking.Abstractions.Repositories;
using DistributedLocking.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using TestHelpers.Attributes;
using Xunit;

namespace Extensions.DependencyInjection.UnitTests
{
    public class ServiceCollectionExtensions_Should
    {
        [Fact]
        public void Throw_When_PassedServicesCollectionIsNull()
        {
            IServiceCollection serviceCollection = null;
            Assert.Throws<ArgumentNullException>(() => serviceCollection.AddDistributedLocking());
        }

        [Fact]
        public void Throw_When_Repository_IsNotRegistered()
        {
            Assert.Throws<AggregateException>(() => new ServiceCollection()
                .AddDistributedLocking()
                .BuildServiceProvider(new ServiceProviderOptions
                {
                    ValidateOnBuild = true,
                    ValidateScopes = true
                }));
        }
        
        [Theory]
        [AutoMoqData]
        public void NotThrow_When_Repository_IsRegistered(IDistributedLocksRepository repository)
        {
            _ = new ServiceCollection()
                .AddDistributedLocking()
                .AddSingleton(repository)
                .BuildServiceProvider(new ServiceProviderOptions
                {
                    ValidateOnBuild = true,
                    ValidateScopes = true
                });
        }
    }
}