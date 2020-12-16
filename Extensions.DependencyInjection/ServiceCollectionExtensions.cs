using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TheName.DistributedLocking.Abstractions;

namespace TheName.DistributedLocking.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDistributedLocking(this IServiceCollection services)
        {
            services.TryAddTransient<IDistributedLockAcquirer, DistributedLockAcquirer>();

            return services;
        }
    }
}