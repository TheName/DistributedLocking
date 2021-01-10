using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.Abstractions.Initializers;
using DistributedLocking.Abstractions.Repositories;
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

            await DistributedLockUsageSampleAsync(provider);
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

        private static async Task DistributedLockUsageSampleAsync(IServiceProvider provider)
        {
            var lockRepository = provider.GetRequiredService<IDistributedLockRepository>();
            var lockIdentifier = Guid.NewGuid();
            var timeToLive = TimeSpan.FromSeconds(30);
            var acquiringResult = await lockRepository.TryAcquireLockAsync(
                lockIdentifier,
                timeToLive,
                CancellationToken.None);

            if (!acquiringResult.Success)
            {
                throw new Exception("Could not acquire lock!");
            }
            
            Console.WriteLine($"Successfully acquired lock with identifier {lockIdentifier} and ID {acquiringResult.AcquiredLock.LockId}!");

            // the lock will be released automatically
            await using (acquiringResult.AcquiredLock)
            {
                // do some action ...

                var secondAcquiringResult = await lockRepository.TryAcquireAsync(
                    lockIdentifier,
                    timeToLive,
                    CancellationToken.None);
                if (secondAcquiringResult.Success)
                {
                    throw new Exception(
                        "Could acquire lock with the same identifier even though it should not be possible.");
                }

                Console.WriteLine(
                    $"Failed to acquire another lock with the same identifier ({lockIdentifier}) because the first one is still acquired.");
                
                // extent the lock lease
                var extendingResult = await lockRepository.TryExtendAsync(
                    acquiringResult.AcquiredLock,
                    timeToLive,
                    CancellationToken.None);

                if (!extendingResult)
                {
                    throw new Exception("Could not extend lock lease even though it should be possible.");
                }

                Console.WriteLine(
                    $"Successfully extended lease for lock with identifier {lockIdentifier} and ID {acquiringResult.AcquiredLock.LockId}.");

                // do some more action ...
            }
            
            var thirdTimeToLive = TimeSpan.FromSeconds(3);
            var thirdAcquiringResult = await lockRepository.TryAcquireLockAsync(
                lockIdentifier,
                thirdTimeToLive,
                CancellationToken.None);

            if (!thirdAcquiringResult.Success)
            {
                throw new Exception("Could not acquire lock!");
            }

            var newLockId = thirdAcquiringResult.AcquiredLock.LockId;
            Console.WriteLine($"Successfully acquired lock with identifier {lockIdentifier} and ID {newLockId}!");
            await Task.Delay(thirdTimeToLive);
            
            var extendingNewLockResult = await lockRepository.TryExtendAsync(
                lockIdentifier,
                newLockId,
                timeToLive,
                CancellationToken.None);

            if (extendingNewLockResult)
            {
                throw new Exception("Extended new lock lease even though it should not be possible.");
            }

            Console.WriteLine(
                $"Could not extend lease for new lock with identifier {lockIdentifier} and ID {newLockId} because the lease (TTL / time to live) has expired.");

            var releasingNewLockResult = await lockRepository.TryReleaseAsync(
                lockIdentifier,
                newLockId,
                CancellationToken.None);

            if (releasingNewLockResult)
            {
                throw new Exception("Released lock even though it should not be possible.");
            }
            
            Console.WriteLine(
                $"Could not release new lock with identifier {lockIdentifier} and ID {newLockId} because the lease (TTL / time to live) has expired and thus it was automatically released.");
        }

        private static async Task RawRepositoryUsageSampleAsync(IServiceProvider provider)
        {
            var lockRepository = provider.GetRequiredService<IDistributedLockRepository>();
            var lockIdentifier = Guid.NewGuid();
            var timeToLive = TimeSpan.FromSeconds(30);
            var acquiringResult = await lockRepository.TryAcquireAsync(
                lockIdentifier,
                timeToLive,
                CancellationToken.None);

            if (!acquiringResult.Success)
            {
                throw new Exception("Could not acquire lock!");
            }

            var lockId = acquiringResult.AcquiredLockId;
            Console.WriteLine($"Successfully acquired lock with identifier {lockIdentifier} and ID {lockId}!");

            var secondAcquiringResult = await lockRepository.TryAcquireAsync(
                lockIdentifier,
                timeToLive,
                CancellationToken.None);
            if (secondAcquiringResult.Success)
            {
                throw new Exception(
                    "Could acquire lock with the same identifier even though it should not be possible.");
            }

            Console.WriteLine(
                $"Failed to acquire another lock with the same identifier ({lockIdentifier}) because the first one is still acquired.");

            var extendingResult = await lockRepository.TryExtendAsync(
                lockIdentifier,
                lockId,
                timeToLive,
                CancellationToken.None);

            if (!extendingResult)
            {
                throw new Exception("Could not extend lock lease even though it should be possible.");
            }

            Console.WriteLine(
                $"Successfully extended lease for lock with identifier {lockIdentifier} and ID {lockId}.");

            var releasingResult = await lockRepository.TryReleaseAsync(
                lockIdentifier,
                lockId,
                CancellationToken.None);

            if (!releasingResult)
            {
                throw new Exception("Could not release lock even though it should be possible.");
            }
            
            Console.WriteLine(
                $"Successfully released lock with identifier {lockIdentifier} and ID {lockId}.");

            var thirdTimeToLive = TimeSpan.FromSeconds(3);
            var thirdAcquiringResult = await lockRepository.TryAcquireAsync(
                lockIdentifier,
                thirdTimeToLive,
                CancellationToken.None);

            if (!thirdAcquiringResult.Success)
            {
                throw new Exception("Could not acquire lock!");
            }

            var newLockId = thirdAcquiringResult.AcquiredLockId;
            Console.WriteLine($"Successfully acquired lock with identifier {lockIdentifier} and ID {newLockId}!");
            await Task.Delay(thirdTimeToLive);
            
            var extendingNewLockResult = await lockRepository.TryExtendAsync(
                lockIdentifier,
                newLockId,
                timeToLive,
                CancellationToken.None);

            if (extendingNewLockResult)
            {
                throw new Exception("Extended new lock lease even though it should not be possible.");
            }

            Console.WriteLine(
                $"Could not extend lease for new lock with identifier {lockIdentifier} and ID {newLockId} because the lease (TTL / time to live) has expired.");

            var releasingNewLockResult = await lockRepository.TryReleaseAsync(
                lockIdentifier,
                newLockId,
                CancellationToken.None);

            if (releasingNewLockResult)
            {
                throw new Exception("Released lock even though it should not be possible.");
            }
            
            Console.WriteLine(
                $"Could not release new lock with identifier {lockIdentifier} and ID {newLockId} because the lease (TTL / time to live) has expired and thus it was automatically released.");
        }
    }
}