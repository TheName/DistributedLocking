using System;
using System.Threading;
using System.Threading.Tasks;
using DbUp;
using DistributedLocking.Abstractions;
using DistributedLocking.Extensions;
using DistributedLocking.Extensions.Repositories.SqlServer.DependencyInjection;
using DistributedLocking.Repositories.SqlServer.Abstractions.Configuration;
using DistributedLocking.Repositories.SqlServer.Migrations.DbUp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DistributedLocking.Repositories.SqlServer.Samples
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // add configuration file
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, false)
                .Build();
            
            var provider = new ServiceCollection()
                .AddSingleton<IConfiguration>(configuration)
                .AddSqlServerDistributedLocking()
                .BuildServiceProvider();
            
            // prepare the database by applying distributed lock SQL Server migrations
            ApplyMigrations(provider);

            var facade = provider.GetRequiredService<IDistributedLockFacade>();

            var resourceId = "Resource ID"; 
            var (success, distributedLock) = await facade.TryAcquireAsync(
                resourceId,
                TimeSpan.FromMinutes(5),
                CancellationToken.None);

            if (!success)
            {
                throw new Exception("Trying to acquire distributed lock failed!");
            }
            
            Console.WriteLine($"Successfully acquired lock with resource id \"{resourceId}\" and lock id \"{distributedLock.Id}\"");

            // acquiring a new lock for same resource id will now fail:
            (success, _) = await facade.TryAcquireAsync(
                resourceId,
                TimeSpan.FromMinutes(5),
                CancellationToken.None);

            if (success)
            {
                throw new Exception("Trying to acquire a new lock for same resource id should fail!");
            }
            
            Console.WriteLine($"Trying to acquire a new lock with resource id \"{resourceId}\" failed, because that resource id is still locked.");
            
            // extend the lock, if you need more time (keep in mind it OVERWRITES the previous TTL, if the lock is still active!
            var extendingResult = await distributedLock.TryExtendAsync(TimeSpan.FromSeconds(2), CancellationToken.None);

            if (!extendingResult)
            {
                throw new Exception("Extending the lock should succeed!");
            }

            Console.WriteLine($"Extending already acquired lock with resource id \"{resourceId}\" succeeded.");
            
            // let's simulate longer running work, than expected:
            await Task.Delay(TimeSpan.FromSeconds(3));
            
            // now acquiring a new lock should succeed, because the TTL of the previous one has expired:
            var (newDistributedLockSuccess, newDistributedLock) = await facade.TryAcquireAsync(
                resourceId,
                TimeSpan.FromMinutes(5),
                CancellationToken.None);

            if (!newDistributedLockSuccess)
            {
                throw new Exception("Trying to acquire the new lock should succeed!");
            }
            
            Console.WriteLine($"Successfully acquired a new lock with resource id \"{resourceId}\" and lock id \"{newDistributedLock.Id}\" because the old one has expired.");
            
            // notice that releasing the old lock will now fail (since it's already expired):
            var releasingResult = await facade.TryReleaseAsync(
                distributedLock.ResourceId,
                distributedLock.Id,
                CancellationToken.None);

            if (releasingResult)
            {
                throw new Exception("Releasing the old lock should fail!");
            }
            
            Console.WriteLine($"Releasing old lock with resource id \"{resourceId}\" and id \"{distributedLock.Id}\" failed because it has already expired.");
            
            // however releasing the new lock will succeed:
            releasingResult = await facade.TryReleaseAsync(
                newDistributedLock.ResourceId,
                newDistributedLock.Id,
                CancellationToken.None);
            
            if (!releasingResult)
            {
                throw new Exception("Releasing the new lock should succeed!");
            }
            
            Console.WriteLine($"Releasing new lock with resource id \"{resourceId}\" and id \"{newDistributedLock.Id}\" succeeded.");
            
            // you can also use extensions:
            var newlyAcquiredLock = await facade.AcquireAsync(
                resourceId,
                TimeSpan.FromMinutes(5),
                CancellationToken.None);
            
            Console.WriteLine($"Successfully acquired a new lock with resource id \"{resourceId}\" and id \"{newlyAcquiredLock.Id}\" because the old one was released.");

            await using (newlyAcquiredLock)
            {
                // some long running process.
            }
            
            // now the lock is already released by IAsyncDisposable.
            Console.WriteLine($"Successfully released new lock with resource id \"{resourceId}\" and id \"{newlyAcquiredLock.Id}\" via IAsyncDisposable.");
            
            // as a proof, we can acquire same resource id again: 
            var (anotherLockSuccess, anotherLock) = await facade.TryAcquireAsync(resourceId, TimeSpan.FromMinutes(5), CancellationToken.None);
            if (!anotherLockSuccess)
            {
                throw new Exception("Trying to acquire should succeed!");
            }
            
            await using (anotherLock)
            {
                Console.WriteLine($"Successfully acquired yet another lock with resource id \"{resourceId}\" and id \"{anotherLock.Id}\" because the old one was released via IAsyncDisposable.");
            }
        }

        private static void ApplyMigrations(IServiceProvider provider)
        {
            var sqlConfiguration = provider.GetRequiredService<ISqlServerDistributedLockConfiguration>();
            
            EnsureDatabase.For.SqlDatabase(sqlConfiguration.ConnectionString);

            var upgrader = DeployChanges.To
                .SqlDatabase(sqlConfiguration.ConnectionString)
                .WithDistributedLockingSqlServerMigrations()
                .LogToConsole()
                .Build();
            
            var result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {
                throw new Exception("Migrations unsuccessful!");
            }
        }
    }
}