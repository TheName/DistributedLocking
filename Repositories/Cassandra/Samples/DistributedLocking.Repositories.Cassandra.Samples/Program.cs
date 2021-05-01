using System;
using System.Threading;
using System.Threading.Tasks;
using Cassandra;
using DistributedLocking.Abstractions;
using DistributedLocking.Extensions;
using DistributedLocking.Extensions.Repositories.Cassandra.DependencyInjection;
using DistributedLocking.Repositories.Cassandra.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace DistributedLocking.Repositories.SqlServer.Samples
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var keyspace = Guid.NewGuid().ToString().Replace("-", "_");
            var cluster = Cluster
                .Builder()
                .AddContactPoint("localhost")
                .WithDefaultKeyspace(keyspace)
                .Build();

            var session = cluster.ConnectAndCreateDefaultKeyspaceIfNotExists();

            var provider = new ServiceCollection()
                .AddCassandraDistributedLocking()
                .AddSingleton(session)
                .BuildServiceProvider();

            try
            {
                await Sample(provider);
            }
            finally
            {
                await session.ExecuteAsync(new SimpleStatement($"DROP KEYSPACE IF EXISTS \"{keyspace}\""));
            }
        }
        
        private static async Task Sample(IServiceProvider provider)
        {
            // prepare the database by applying distributed lock Cassandra migrations
            await ApplyMigrations(provider);

            var distributedLockProvider = provider.GetRequiredService<IDistributedLockProvider>();

            var resourceId = "Resource ID"; 
            var distributedLock = await distributedLockProvider.TryAcquireAsync(
                resourceId,
                TimeSpan.FromMinutes(5),
                CancellationToken.None);

            if (distributedLock == null)
            {
                throw new Exception("Trying to acquire distributed lock failed!");
            }
            
            Console.WriteLine($"Successfully acquired lock with resource id \"{resourceId}\" and lock id \"{distributedLock.Id}\"");

            // acquiring a new lock for same resource id will now fail:
            var otherDistributedLock = await distributedLockProvider.TryAcquireAsync(
                resourceId,
                TimeSpan.FromMinutes(5),
                CancellationToken.None);

            if (otherDistributedLock != null)
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
            var newDistributedLock = await distributedLockProvider.TryAcquireAsync(
                resourceId,
                TimeSpan.FromMinutes(5),
                CancellationToken.None);

            if (newDistributedLock == null)
            {
                throw new Exception("Trying to acquire the new lock should succeed!");
            }
            
            Console.WriteLine($"Successfully acquired a new lock with resource id \"{resourceId}\" and lock id \"{newDistributedLock.Id}\" because the old one has expired.");
            
            // notice that releasing the old lock will now fail (since it's already expired):
            var releasingResult = await distributedLock.TryReleaseAsync(CancellationToken.None);

            if (releasingResult)
            {
                throw new Exception("Releasing the old lock should fail!");
            }
            
            Console.WriteLine($"Releasing old lock with resource id \"{resourceId}\" and id \"{distributedLock.Id}\" failed because it has already expired.");
            
            // however releasing the new lock will succeed:
            releasingResult = await newDistributedLock.TryReleaseAsync(CancellationToken.None);
            
            if (!releasingResult)
            {
                throw new Exception("Releasing the new lock should succeed!");
            }
            
            Console.WriteLine($"Releasing new lock with resource id \"{resourceId}\" and id \"{newDistributedLock.Id}\" succeeded.");
            
            // you can also use extensions:
            var newlyAcquiredLock = await distributedLockProvider.AcquireAsync(
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
            var anotherLock = await distributedLockProvider.TryAcquireAsync(resourceId, TimeSpan.FromMinutes(5), CancellationToken.None);
            if (anotherLock == null)
            {
                throw new Exception("Trying to acquire should succeed!");
            }
            
            await using (anotherLock)
            {
                Console.WriteLine($"Successfully acquired yet another lock with resource id \"{resourceId}\" and id \"{anotherLock.Id}\" because the old one was released via IAsyncDisposable.");
            }
        }

        private static async Task ApplyMigrations(IServiceProvider provider)
        {
            var session = provider.GetRequiredService<ISession>();
            var scriptsProvider = new CassandraDistributedLocksRepositoryMigrationsProvider(session.Keyspace);
            var migrationScripts = await scriptsProvider.GetMigrationsAsync();
            foreach (var migrationScript in migrationScripts)
            {
                await session.ExecuteAsync(new SimpleStatement(migrationScript.Content));
            }
        }
    }
}