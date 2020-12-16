using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using TheName.DistributedLocking.Abstractions.Repositories;
using TheName.DistributedLocking.SqlServer.Abstractions.Configuration;
using TheName.DistributedLocking.SqlServer.Abstractions.Helpers;
using TheName.DistributedLocking.SqlServer.Configuration;
using TheName.DistributedLocking.SqlServer.Helpers;
using TheName.DistributedLocking.SqlServer.Repositories;

namespace TheName.DistributedLocking.Extensions.DependencyInjection.SqlServer
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSqlServerDistributedLocking(this IServiceCollection services)
        {
            services.AddDistributedLocking();
            services.TryAddTransient<ISqlClient>(provider =>
                new SqlClient(provider.GetRequiredService<ISqlServerDistributedLockConfiguration>()));

            services.TryAddTransient<ISqlDataDefinitionLanguageExecutor>(provider =>
                new SqlDataDefinitionLanguageExecutor(provider.GetRequiredService<ISqlClient>()));

            services.TryAddTransient<ISqlDistributedLocksTable>(provider =>
                new SqlDistributedLocksTable(
                    provider.GetRequiredService<ISqlClient>(),
                    provider.GetRequiredService<ISqlDataDefinitionLanguageExecutor>()));

            services.TryAddTransient<IDistributedLockRepository>(provider =>
                new SqlServerDistributedLockRepository(
                    provider.GetRequiredService<ISqlDistributedLocksTable>(),
                    provider.GetRequiredService<ISqlServerDistributedLockConfiguration>()));

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