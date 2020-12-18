using Microsoft.Extensions.DependencyInjection;
using TheName.DistributedLocking.Extensions.DependencyInjection.SqlServer;
using Xunit;

namespace Extensions.DependencyInjection.SqlServer.UnitTests
{
    public class ServiceCollectionExtensions_Should
    {
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