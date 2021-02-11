using System;
using DistributedLocking.Abstractions.Repositories;
using DistributedLocking.Extensions.DependencyInjection;
using DistributedLocking.Repositories.SqlServer;
using DistributedLocking.Repositories.SqlServer.Abstractions.Configuration;
using DistributedLocking.Repositories.SqlServer.Configuration;
using DistributedLocking.Repositories.SqlServer.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace DistributedLocking.Extensions.Repositories.SqlServer.DependencyInjection
{
    /// <summary>
    /// The <see cref="IServiceCollection"/> extensions.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds distributed locking default implementation with SQL Server implementation.
        /// </summary>
        /// <param name="services">
        /// The <see cref="IServiceCollection"/>.
        /// </param>
        /// <returns>
        /// The <see cref="IServiceCollection"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="services"/> is null.
        /// </exception>
        public static IServiceCollection AddSqlServerDistributedLocking(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            
            services.AddDistributedLocking();

            services.TryAddTransient<IDistributedLocksRepository>(provider =>
            {
                var configuration = provider.GetRequiredService<ISqlServerDistributedLockConfiguration>();
                return new SqlServerDistributedLocksRepository(new SqlClient(new SqlConnectionFactory(configuration)));
            });

            services.AddOptions<SqlServerDistributedLockConfiguration>()
                .BindConfiguration(nameof(SqlServerDistributedLockConfiguration))
                .Validate(configuration => !string.IsNullOrWhiteSpace(configuration.ConnectionString));

            services.TryAddSingleton<ISqlServerDistributedLockConfiguration>(provider =>
                provider.GetRequiredService<IOptions<SqlServerDistributedLockConfiguration>>().Value);

            return services;
        }
    }
}