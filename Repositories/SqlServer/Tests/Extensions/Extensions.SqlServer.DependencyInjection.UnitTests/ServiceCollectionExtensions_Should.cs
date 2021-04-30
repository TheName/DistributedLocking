using System;
using DistributedLocking.Extensions.Repositories.SqlServer.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Extensions.SqlServer.DependencyInjection.UnitTests
{
    public class ServiceCollectionExtensions_Should
    {
        [Fact]
        public void Throw_When_PassedServicesCollectionIsNull()
        {
            IServiceCollection serviceCollection = null;
            Assert.Throws<ArgumentNullException>(() => serviceCollection.AddSqlServerDistributedLocking());
        }
        
        [Fact]
        public void NotThrow()
        {
            _ = new ServiceCollection()
                .AddSqlServerDistributedLocking()
                .BuildServiceProvider(new ServiceProviderOptions
                {
                    ValidateOnBuild = true,
                    ValidateScopes = true
                });
        }
    }
}