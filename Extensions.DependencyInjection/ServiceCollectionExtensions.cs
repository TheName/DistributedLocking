using System;
using DistributedLocking.Abstractions.Facades;
using DistributedLocking.Abstractions.Facades.Retries;
using DistributedLocking.Abstractions.Repositories.Initializers;
using DistributedLocking.Facades;
using DistributedLocking.Facades.Retries;
using DistributedLocking.Repositories.Initializers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DistributedLocking.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDistributedLocking(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            
            services.TryAddSingleton<IDistributedLockRepositoryInitializer, DistributedLockRepositoryInitializer>();
            services.TryAddTransient<IDistributedLockFacade, DistributedLockFacade>();
            services.TryAddTransient<IRetryExecutor, RetryExecutor>();

            return services;
        }
    }
}