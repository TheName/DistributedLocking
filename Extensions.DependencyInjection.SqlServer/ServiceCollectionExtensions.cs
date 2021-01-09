using DistributedLocking.Abstractions.Factories;
using DistributedLocking.SqlServer.Abstractions.Configuration;
using DistributedLocking.SqlServer.Configuration;
using DistributedLocking.SqlServer.Factories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace DistributedLocking.Extensions.DependencyInjection.SqlServer
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSqlServerDistributedLocking(this IServiceCollection services)
        {
            services
                .AddDistributedLocking()
                .TryAddTransient<IDistributedLockRepositoryFactory, SqlServerDistributedLockFactory>();

            services.TryAddTransient<IDistributedLockRepositoryManagerFactory, SqlServerDistributedLockFactory>();

            services.AddOptions<SqlServerDistributedLockConfiguration>()
                .BindConfiguration(nameof(SqlServerDistributedLockConfiguration))
                .Validate(configuration => !string.IsNullOrWhiteSpace(configuration.ConnectionString) &&
                                           !string.IsNullOrWhiteSpace(configuration.SchemaName) &&
                                           !string.IsNullOrWhiteSpace(configuration.TableName));

            services.TryAddSingleton<ISqlServerDistributedLockConfiguration>(provider =>
                provider.GetRequiredService<IOptions<SqlServerDistributedLockConfiguration>>().Value);

            return services;
        }
    }
}