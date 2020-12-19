using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TheName.DistributedLocking.Abstractions;
using TheName.DistributedLocking.Abstractions.Initializers;
using TheName.DistributedLocking.Abstractions.Managers;
using TheName.DistributedLocking.Abstractions.Repositories;
using TheName.DistributedLocking.Initializers;
using TheName.DistributedLocking.Managers;
using TheName.DistributedLocking.Repositories;

namespace TheName.DistributedLocking.Extensions.DependencyInjection
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