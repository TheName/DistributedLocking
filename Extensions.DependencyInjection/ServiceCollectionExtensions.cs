using DistributedLocking.Abstractions.Initializers;
using DistributedLocking.Abstractions.Managers;
using DistributedLocking.Abstractions.Repositories;
using DistributedLocking.Initializers;
using DistributedLocking.Managers;
using DistributedLocking.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DistributedLocking.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDistributedLocking(this IServiceCollection services)
        {
            services.TryAddTransient<IDistributedLockRepositoryManager, DistributedLockRepositoryManagerProxy>();
            services.TryAddTransient<IDistributedLockRepository, DistributedLockRepositoryProxy>();
            services.TryAddSingleton<IDistributedLockRepositoryInitializer, DistributedLockRepositoryInitializer>();
            services.TryAddTransient<IDistributedLockManager, DistributedLockManager>();

            return services;
        }
    }
}