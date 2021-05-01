using System;
using DistributedLocking.Abstractions.Repositories;
using DistributedLocking.Extensions.DependencyInjection;
using DistributedLocking.Repositories.Cassandra;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DistributedLocking.Extensions.Repositories.Cassandra.DependencyInjection
{
    /// <summary>
    /// The <see cref="IServiceCollection"/> extensions.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds distributed locking default implementation with Cassandra implementation.
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
        public static IServiceCollection AddCassandraDistributedLocking(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            
            services.AddDistributedLocking();
            services.TryAddSingleton<IDistributedLocksRepository, CassandraDistributedLockRepository>();

            return services;
        }
    }
}