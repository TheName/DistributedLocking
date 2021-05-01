using System;
using Cassandra;
using DistributedLocking.Extensions.Repositories.Cassandra.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using TestHelpers.Attributes;
using Xunit;

namespace Extensions.Cassandra.DependencyInjection.UnitTests
{
    public class ServiceCollectionExtensions_Should
    {
        [Fact]
        public void Throw_When_PassedServicesCollectionIsNull()
        {
            IServiceCollection serviceCollection = null;
            Assert.Throws<ArgumentNullException>(() => serviceCollection.AddCassandraDistributedLocking());
        }
        
        [Fact]
        public void Throw_When_SessionIsNotRegistered()
        {
            Assert.Throws<AggregateException>(() => new ServiceCollection()
                .AddCassandraDistributedLocking()
                .BuildServiceProvider(new ServiceProviderOptions
                {
                    ValidateOnBuild = true,
                    ValidateScopes = true
                }));
        }
        
        [Theory]
        [AutoMoqData]
        public void NotThrow_When_SessionIsRegistered(ISession session)
        {
            _ = new ServiceCollection()
                .AddCassandraDistributedLocking()
                .AddSingleton(session)
                .BuildServiceProvider(new ServiceProviderOptions
                {
                    ValidateOnBuild = true,
                    ValidateScopes = true
                });
        }
    }
}