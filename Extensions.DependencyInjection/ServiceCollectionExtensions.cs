using DistributedLocking.Abstractions.Initializers;
using DistributedLocking.Abstractions.Managers;
using DistributedLocking.Abstractions.Retries;
using DistributedLocking.Initializers;
using DistributedLocking.Managers;
using DistributedLocking.Retries;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DistributedLocking.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDistributedLocking(this IServiceCollection services)
        {
            services.TryAddSingleton<IDistributedLockRepositoryInitializer, DistributedLockRepositoryInitializer>();
            services.TryAddTransient<IDistributedLockManager, DistributedLockManager>();
            services.TryAddTransient<IRetryExecutor, RetryExecutor>();

            return services;
        }
    }
}