using DistributedLocking.Abstractions.Managers;
using DistributedLocking.Abstractions.Repositories;
using DistributedLocking.SqlServer.Abstractions.Configuration;
using DistributedLocking.SqlServer.Configuration;
using DistributedLocking.SqlServer.Helpers;
using DistributedLocking.SqlServer.Managers;
using DistributedLocking.SqlServer.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace DistributedLocking.Extensions.DependencyInjection.SqlServer
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSqlServerDistributedLocking(this IServiceCollection services)
        {
            services.AddDistributedLocking();

            services.TryAddTransient<IDistributedLockRepository>(provider =>
            {
                var configuration = provider.GetRequiredService<ISqlServerDistributedLockConfiguration>();
                return new SqlServerDistributedLockRepository(
                    new SqlDistributedLocksTable(new SqlClient(new SqlConnectionFactory(configuration))),
                    configuration);
            });
            services.TryAddTransient<IDistributedLockRepositoryManager>(provider =>
            {
                var configuration = provider.GetRequiredService<ISqlServerDistributedLockConfiguration>();
                return new SqlServerDistributedLockRepositoryManager(
                    new SqlDistributedLocksTableManager(
                        new SqlDataDefinitionLanguageExecutor(new SqlClient(new SqlConnectionFactory(configuration)))),
                    configuration);
            });

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