using System;
using DistributedLocking.Abstractions.Managers;
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
        public void Throw_When_RepositoryFactory_And_RepositoryManagerFactory_AreNotRegistered()
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
        public void NotThrow_When_RepositoryFactory_And_RepositoryManagerFactory_AreRegistered(
            IDistributedLockRepository repository,
            IDistributedLockRepositoryManager repositoryManager)
        {
            _ = new ServiceCollection()
                .AddDistributedLocking()
                .AddSingleton(repository)
                .AddSingleton(repositoryManager)
                .BuildServiceProvider(new ServiceProviderOptions
                {
                    ValidateOnBuild = true,
                    ValidateScopes = true
                });
        }
    }
}