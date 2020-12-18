using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using TheName.DistributedLocking.Abstractions.Factories;
using TheName.DistributedLocking.SqlServer.Abstractions.Configuration;
using TheName.DistributedLocking.SqlServer.Configuration;
using TheName.DistributedLocking.SqlServer.Factories;

namespace TheName.DistributedLocking.Extensions.DependencyInjection.SqlServer
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