using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.Abstractions.Facades;
using DistributedLocking.Abstractions.Initializers;
using DistributedLocking.Abstractions.Repositories;
using DistributedLocking.Abstractions.Repositories.Exceptions;
using DistributedLocking.Extensions.Abstractions.Facades;
using DistributedLocking.Extensions.Abstractions.Repositories;
using DistributedLocking.Extensions.SqlServer.DependencyInjection;
using DistributedLocking.SqlServer.Abstractions.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SqlServerConsoleApplicationSample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var provider = CreateServiceProvider();

            await InitializeAsync(provider);

            await DistributedLockWithRepositoryUsageSampleAsync(provider);
            await DistributedLockWithFacadeUsageSampleAsync(provider);
            await RawRepositoryUsageSampleAsync(provider);
        }

        private static IServiceProvider CreateServiceProvider()
        {
            var services = new ServiceCollection();
            
            // create configuration with appsettings file
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, false)
                .Build();
            services.AddSingleton<IConfiguration>(configuration);
            
            // add sql server distributed lock
            services.AddSqlServerDistributedLocking();
            
            return services.BuildServiceProvider();
        }

        private static async Task InitializeAsync(IServiceProvider provider)
        {
            // as step 0 we need to ensure the database exists
            // something that usually should be done beforehand
            var sqlConfiguration = provider.GetRequiredService<ISqlServerDistributedLockConfiguration>();
            await CreateDatabaseIfNotExistsAsync(sqlConfiguration);

            // first, we need to ensure that the repository is initialized
            // this needs to be done just once per application run (start)
            // it essentially creates required tables if they don't exist yet in the SQL server
            var initializer = provider.GetRequiredService<IDistributedLockRepositoryInitializer>();
            await initializer.InitializeAsync(CancellationToken.None);
        }

        private static async Task CreateDatabaseIfNotExistsAsync(ISqlServerDistributedLockConfiguration sqlConfiguration)
        {
            var sqlConnectionBuilder = new SqlConnectionStringBuilder(sqlConfiguration.ConnectionString);
            var createDatabaseIfNotExistsCommand =
                $"IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = '{sqlConnectionBuilder.InitialCatalog}') " +
                "BEGIN " +
                $"CREATE DATABASE {sqlConnectionBuilder.InitialCatalog}; " +
                "END;";
            
            // reset initial catalog in case the DB does not exist yet.
            sqlConnectionBuilder.InitialCatalog = string.Empty;

            await using var connection = new SqlConnection(sqlConnectionBuilder.ConnectionString);
            var command = new SqlCommand(createDatabaseIfNotExistsCommand, connection);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        private static async Task DistributedLockWithRepositoryUsageSampleAsync(IServiceProvider provider)
        {
            var repository = provider.GetRequiredService<IDistributedLockRepository>();
            
            var identifier = Guid.NewGuid();
            var timeToLive = TimeSpan.FromSeconds(30);

            // try acquiring
            var (success, distributedLock) = await repository.TryAcquireLockAsync(
                identifier,
                timeToLive,
                CancellationToken.None);

            // acquiring can fail;
            // a common case for failure would be lock identifier being currently used by another lock
            // of course it can also by any other reason
            if (!success)
            {
                throw new Exception("Could not acquire lock!");
            }

            Console.WriteLine($"Successfully acquired lock {distributedLock}!");
            
            // there are two ways to release lock;
            //  - dispose the lock itself
            //  - explicitly call TryRelease on repository and pass the lock

            // the lock will be released using disposing method
            await using (distributedLock)
            {
                // do some action ...

                // trying to acquire a lock for the same identifier will fail,
                // because the current lock is using it.
                var secondAcquiringResult = await repository.TryAcquireAsync(
                    identifier,
                    timeToLive,
                    CancellationToken.None);
                if (secondAcquiringResult.Success)
                {
                    throw new Exception(
                        "Could acquire lock with the same identifier even though it should not be possible.");
                }

                Console.WriteLine(
                    $"Failed to acquire another lock with the same identifier ({identifier}) because the first one is still acquired.");
                
                // if you realize that the work might take longer than the original TTL (time to live) / lease time
                // you can extend the lock.
                var extendingResult = await repository.TryExtendAsync(
                    distributedLock,
                    timeToLive,
                    CancellationToken.None);

                if (!extendingResult)
                {
                    throw new Exception("Could not extend lock lease even though it should be possible.");
                }

                Console.WriteLine($"Successfully extended lease for lock {distributedLock}.");

                // do some more action ...
            }
            
            var thirdTimeToLive = TimeSpan.FromSeconds(3);
            
            // now, that the identifier has been released, we can acquire a new lock with same identifier
            var thirdAcquiringResult = await repository.TryAcquireLockAsync(
                identifier,
                thirdTimeToLive,
                CancellationToken.None);

            if (!thirdAcquiringResult.Success)
            {
                throw new Exception("Could not acquire lock!");
            }

            var newLockId = thirdAcquiringResult.AcquiredLock.Id;
            Console.WriteLine($"Successfully acquired lock with identifier {identifier} and ID {newLockId}!");
            
            // let's wait for the lock's TTL (time to live) / lease time to expire
            await Task.Delay(thirdTimeToLive);
            
            // now, after TTL has expired we cannot extend the lock anymore as it's already expired.
            var extendingNewLockResult = await repository.TryExtendAsync(
                identifier,
                newLockId,
                timeToLive,
                CancellationToken.None);

            if (extendingNewLockResult)
            {
                throw new Exception("Extended new lock lease even though it should not be possible.");
            }

            Console.WriteLine(
                $"Could not extend lease for new lock with identifier {identifier} and ID {newLockId} because the lease (TTL / time to live) has expired.");

            // releasing the lock after TTL has expired will also not succeed.
            var releasingNewLockResult = await repository.TryReleaseAsync(
                identifier,
                newLockId,
                CancellationToken.None);

            if (releasingNewLockResult)
            {
                throw new Exception("Released lock even though it should not be possible.");
            }
            
            Console.WriteLine(
                $"Could not release new lock with identifier {identifier} and ID {newLockId} because the lease (TTL / time to live) has expired and thus it was automatically released.");
        }

        private static async Task DistributedLockWithFacadeUsageSampleAsync(IServiceProvider provider)
        {
            var distributedLockFacade = provider.GetRequiredService<IDistributedLockFacade>();
            
            var identifier = Guid.NewGuid();
            var timeToLive = TimeSpan.FromSeconds(30);

            // with the facade, you can acquire lock or throw
            var distributedLock = await distributedLockFacade.AcquireAsync(
                identifier,
                timeToLive,
                CancellationToken.None);

            Console.WriteLine($"Successfully acquired lock {distributedLock}!");

            // the lock will be released using disposing method
            await using (distributedLock)
            {
                // do some action ...

                try
                {
                    // trying to acquire a lock for the same identifier will fail,
                    // because the current lock is using it.
                    var _ = await distributedLockFacade.AcquireAsync(
                        identifier,
                        timeToLive,
                        CancellationToken.None);
                    
                    throw new Exception(
                        "Could acquire lock with the same identifier even though it should not be possible.");
                }
                catch (CouldNotAcquireLockException)
                {
                    Console.WriteLine(
                        $"Failed to acquire another lock with the same identifier ({identifier}) because the first one is still acquired.");
                }
                
                // if you realize that the work might take longer than the original TTL (time to live) / lease time
                // you can extend the lock.
                await distributedLockFacade.ExtendAsync(
                    distributedLock,
                    timeToLive,
                    CancellationToken.None);

                Console.WriteLine($"Successfully extended lease for lock {distributedLock}.");

                // do some more action ...
            }
            
            var thirdTimeToLive = TimeSpan.FromSeconds(3);
            
            // now, that the identifier has been released, we can acquire a new lock with same identifier
            var thirdAcquiringDistributedLock = await distributedLockFacade.AcquireAsync(
                identifier,
                thirdTimeToLive,
                CancellationToken.None);

            Console.WriteLine($"Successfully acquired lock {thirdAcquiringDistributedLock}!");
            
            // let's wait for the lock's TTL (time to live) / lease time to expire
            await Task.Delay(thirdTimeToLive);
            
            try
            {
                // now, after TTL has expired we cannot extend the lock anymore as it's already expired.
                await distributedLockFacade.ExtendAsync(
                    thirdAcquiringDistributedLock,
                    timeToLive,
                    CancellationToken.None);
                
                throw new Exception("Extended new lock lease even though it should not be possible.");
            }
            catch (CouldNotExtendLockException)
            {
                Console.WriteLine(
                    $"Could not extend lease for new lock {thirdAcquiringDistributedLock} because the lease (TTL / time to live) has expired.");
            }

            try
            {
                // releasing the lock after TTL has expired will also not succeed.
                await distributedLockFacade.ReleaseAsync(
                    thirdAcquiringDistributedLock,
                    CancellationToken.None);
                
                throw new Exception("Released lock even though it should not be possible.");
            }
            catch (CouldNotReleaseLockException)
            {
                Console.WriteLine(
                    $"Could not release new lock {thirdAcquiringDistributedLock} because the lease (TTL / time to live) has expired and thus it was automatically released.");
            }
        }

        private static async Task RawRepositoryUsageSampleAsync(IServiceProvider provider)
        {
            var repository = provider.GetRequiredService<IDistributedLockRepository>();
            
            var identifier = Guid.NewGuid();
            var timeToLive = TimeSpan.FromSeconds(30);
            
            // try acquiring
            var (success, lockId) = await repository.TryAcquireAsync(
                identifier,
                timeToLive,
                CancellationToken.None);

            // acquiring can fail;
            // a common case for failure would be lock identifier being currently used by another lock
            // of course it can also by any other reason
            if (!success)
            {
                throw new Exception("Could not acquire lock!");
            }

            Console.WriteLine($"Successfully acquired lock with identifier {identifier} and ID {lockId}!");

            // trying to acquire a lock for the same identifier will fail,
            // because the current lock is using it.
            var secondAcquiringResult = await repository.TryAcquireAsync(
                identifier,
                timeToLive,
                CancellationToken.None);
            if (secondAcquiringResult.Success)
            {
                throw new Exception(
                    "Could acquire lock with the same identifier even though it should not be possible.");
            }

            Console.WriteLine(
                $"Failed to acquire another lock with the same identifier ({identifier}) because the first one is still acquired.");

            // if you realize that the work might take longer than the original TTL (time to live) / lease time
            // you can extend the lock.
            var extendingResult = await repository.TryExtendAsync(
                identifier,
                lockId,
                timeToLive,
                CancellationToken.None);

            if (!extendingResult)
            {
                throw new Exception("Could not extend lock lease even though it should be possible.");
            }

            Console.WriteLine(
                $"Successfully extended lease for lock with identifier {identifier} and ID {lockId}.");

            // you can wait for the TTL to expire or release the lock explicitly
            var releasingResult = await repository.TryReleaseAsync(
                identifier,
                lockId,
                CancellationToken.None);

            if (!releasingResult)
            {
                throw new Exception("Could not release lock even though it should be possible.");
            }
            
            Console.WriteLine(
                $"Successfully released lock with identifier {identifier} and ID {lockId}.");

            var thirdTimeToLive = TimeSpan.FromSeconds(3);
            
            // now, that the identifier has been released, we can acquire a new lock with same identifier
            var thirdAcquiringResult = await repository.TryAcquireAsync(
                identifier,
                thirdTimeToLive,
                CancellationToken.None);

            if (!thirdAcquiringResult.Success)
            {
                throw new Exception("Could not acquire lock!");
            }

            var newLockId = thirdAcquiringResult.AcquiredLockId;
            Console.WriteLine($"Successfully acquired lock with identifier {identifier} and ID {newLockId}!");
            
            // let's wait for the lock's TTL (time to live) / lease time to expire
            await Task.Delay(thirdTimeToLive);
            
            // now, after TTL has expired we cannot extend the lock anymore as it's already expired.
            var extendingNewLockResult = await repository.TryExtendAsync(
                identifier,
                newLockId,
                timeToLive,
                CancellationToken.None);

            if (extendingNewLockResult)
            {
                throw new Exception("Extended new lock lease even though it should not be possible.");
            }

            Console.WriteLine(
                $"Could not extend lease for new lock with identifier {identifier} and ID {newLockId} because the lease (TTL / time to live) has expired.");

            // releasing the lock after TTL has expired will also not succeed.
            var releasingNewLockResult = await repository.TryReleaseAsync(
                identifier,
                newLockId,
                CancellationToken.None);

            if (releasingNewLockResult)
            {
                throw new Exception("Released lock even though it should not be possible.");
            }
            
            Console.WriteLine(
                $"Could not release new lock with identifier {identifier} and ID {newLockId} because the lease (TTL / time to live) has expired and thus it was automatically released.");
        }
    }
}