using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.Abstractions;
using DistributedLocking.Abstractions.Repositories;
using DistributedLocking.SqlServer.IntegrationTests.CollectionDefinitions;
using DistributedLocking.SqlServer.IntegrationTests.Fixtures;
using TestHelpers.Attributes;
using Xunit;
using Xunit.Extensions.Ordering;

namespace DistributedLocking.SqlServer.IntegrationTests.Repositories
{
    [Collection(nameof(SqlDatabaseConnectionDefinition)), Order(1)]
    public class SqlServerDistributedLockRepository_Should
    {
        private readonly SqlDatabaseFixture _sqlDatabaseFixture;

        private IDistributedLocksRepository DistributedLockRepository =>
            _sqlDatabaseFixture.GetService<IDistributedLocksRepository>();

        public SqlServerDistributedLockRepository_Should(SqlDatabaseFixture sqlDatabaseFixture)
        {
            _sqlDatabaseFixture = sqlDatabaseFixture ?? throw new ArgumentNullException(nameof(sqlDatabaseFixture));
        }

        [Theory]
        [AutoMoqData]
        public async Task AcquireLock(
            DistributedLockResourceId resourceId,
            DistributedLockId lockId,
            DistributedLockTimeToLive timeToLive)
        {
            var success = await DistributedLockRepository.TryInsert(
                resourceId,
                lockId,
                timeToLive,
                CancellationToken.None);
            
            Assert.True(success);
        }

        [Theory]
        [AutoMoqWithInlineData(900)]
        public async Task AcquireLockWithLongResourceId(
            int numberOfCharacters,
            char c,
            DistributedLockId lockId,
            DistributedLockTimeToLive timeToLive)
        {
            var resourceId = new string(c, numberOfCharacters);
            
            var success = await DistributedLockRepository.TryInsert(
                resourceId,
                lockId,
                timeToLive,
                CancellationToken.None);
            
            Assert.True(success);
        }

        [Theory]
        [AutoMoqData]
        public async Task FailToAcquireLock_When_TryingToAcquire_And_ResourceIdIsAlreadyAcquired(
            DistributedLockResourceId resourceId,
            DistributedLockId lockId,
            DistributedLockTimeToLive timeToLive)
        {
            // make sure the timeout will last at least until we try to acquire again
            timeToLive += TimeSpan.FromSeconds(5);
            var success = await DistributedLockRepository.TryInsert(
                resourceId,
                lockId,
                timeToLive,
                CancellationToken.None);
            Assert.True(success);
            
            success = await DistributedLockRepository.TryInsert(
                resourceId,
                lockId,
                timeToLive,
                CancellationToken.None);
            Assert.False(success);
        }

        [Theory]
        [AutoMoqData]
        public async Task AcquireLockOnlyOnOneThread_When_TryingToAcquireLockInParallel(
            DistributedLockResourceId resourceId,
            DistributedLockId lockId,
            DistributedLockTimeToLive timeToLive)
        {
            // make sure the timeout will last at least until all tasks are done
            timeToLive += TimeSpan.FromSeconds(5);
            var tryAcquireTasks = Enumerable.Range(0, 1000)
                .Select(i =>
                    DistributedLockRepository.TryInsert(resourceId, lockId, timeToLive, CancellationToken.None))
                .ToList();
            
            await Task.WhenAll(tryAcquireTasks);
            
            var tryAcquireResults = tryAcquireTasks
                .Select(task => task.Result)
                .ToList();
            
            Assert.Single(tryAcquireResults, x => x);
        }

        [Theory]
        [AutoMoqData]
        public async Task ReleaseAcquiredLock(
            DistributedLockResourceId resourceId,
            DistributedLockId lockId,
            DistributedLockTimeToLive timeToLive)
        {
            // make sure the timeout will last at least until the release is called
            timeToLive += TimeSpan.FromSeconds(5);
            var success = await DistributedLockRepository.TryInsert(
                resourceId,
                lockId,
                timeToLive,
                CancellationToken.None);
            Assert.True(success);

            var result = await DistributedLockRepository.TryDelete(resourceId, lockId, CancellationToken.None);
            
            Assert.True(result);
        }

        [Theory]
        [AutoMoqData]
        public async Task FailToReleaseAcquiredLock_When_TryingToReleaseLock_And_TimeoutHasExpired(
            DistributedLockResourceId resourceId,
            DistributedLockId lockId)
        {
            // make sure the timeout will last no longer than until we try to release it
            const int maxMillisecondsTimeout = 100;
            DistributedLockTimeToLive timeToLive = TimeSpan.FromMilliseconds(new Random().Next(1, maxMillisecondsTimeout));
            var success = await DistributedLockRepository.TryInsert(
                resourceId,
                lockId,
                timeToLive,
                CancellationToken.None);
            Assert.True(success);

            await Task.Delay(maxMillisecondsTimeout);
            var result = await DistributedLockRepository.TryDelete(resourceId, lockId, CancellationToken.None);
            
            Assert.False(result);
        }

        [Theory]
        [AutoMoqData]
        public async Task FailToReleaseAcquiredLock_When_TryingToReleaseLock_And_LockWasNotAcquired(
            DistributedLockResourceId resourceId,
            DistributedLockId id)
        {
            var result = await DistributedLockRepository.TryDelete(resourceId, id, CancellationToken.None);
            
            Assert.False(result);
        }

        [Theory]
        [AutoMoqData]
        public async Task AcquireLock_When_TryingToAcquireLock_And_LockWasAcquiredAndAlreadyReleased(
            DistributedLockResourceId resourceId,
            DistributedLockId lockId,
            DistributedLockTimeToLive timeToLive)
        {
            // make sure the timeout will last at least until the release is called
            timeToLive += TimeSpan.FromSeconds(5);
            var success = await DistributedLockRepository.TryInsert(
                resourceId,
                lockId,
                timeToLive,
                CancellationToken.None);
            Assert.True(success);
            var result = await DistributedLockRepository.TryDelete(resourceId, lockId, CancellationToken.None);
            Assert.True(result);
            
            success = await DistributedLockRepository.TryInsert(
                resourceId,
                lockId,
                timeToLive,
                CancellationToken.None);
            Assert.True(success);
        }

        [Theory]
        [AutoMoqData]
        public async Task FailToReleaseAcquiredLock_When_TryingToReleaseLock_And_LockIdIsCorrectButResourceIdIsNot(
            DistributedLockResourceId resourceId,
            DistributedLockId lockId,
            DistributedLockResourceId anotherResourceId)
        {
            // make sure the timeout will last no longer than until we try to release it
            const int maxMillisecondsTimeout = 100;
            DistributedLockTimeToLive timeToLive = TimeSpan.FromMilliseconds(new Random().Next(1, maxMillisecondsTimeout));
            
            var success = await DistributedLockRepository.TryInsert(
                resourceId,
                lockId,
                timeToLive,
                CancellationToken.None);
            Assert.True(success);

            await Task.Delay(maxMillisecondsTimeout);
            var result = await DistributedLockRepository.TryDelete(
                anotherResourceId,
                lockId,
                CancellationToken.None);
            
            Assert.False(result);
        }

        [Theory]
        [AutoMoqData]
        public async Task FailToReleaseAcquiredLock_When_TryingToReleaseLock_And_LockIdIsIncorrectButResourceIdIsCorrect(
            DistributedLockResourceId resourceId,
            DistributedLockId lockId,
            DistributedLockId anotherId)
        {
            // make sure the timeout will last no longer than until we try to release it
            const int maxMillisecondsTimeout = 100;
            DistributedLockTimeToLive timeToLive = TimeSpan.FromMilliseconds(new Random().Next(1, maxMillisecondsTimeout));
            var success = await DistributedLockRepository.TryInsert(
                resourceId,
                lockId,
                timeToLive,
                CancellationToken.None);
            Assert.True(success);

            await Task.Delay(maxMillisecondsTimeout);
            var result = await DistributedLockRepository.TryDelete(
                resourceId,
                anotherId,
                CancellationToken.None);
            
            Assert.False(result);
        }

        [Theory]
        [AutoMoqData]
        public async Task ExtendAcquiredLock(
            DistributedLockResourceId resourceId,
            DistributedLockId lockId,
            DistributedLockTimeToLive timeToLive)
        {
            // make sure the timeout will last at least until the release is called
            timeToLive += TimeSpan.FromSeconds(5);
            var success = await DistributedLockRepository.TryInsert(
                resourceId,
                lockId,
                timeToLive,
                CancellationToken.None);
            Assert.True(success);

            var result = await DistributedLockRepository.TryUpdateTimeToLiveAsync(
                resourceId,
                lockId,
                timeToLive,
                CancellationToken.None);
            
            Assert.True(result);
        }

        [Theory]
        [AutoMoqData]
        public async Task FailToExtendAcquiredLock_When_TryingToExtendLock_And_TimeoutHasExpired(
            DistributedLockResourceId resourceId,
            DistributedLockId lockId)
        {
            // make sure the timeout will last no longer than until we try to release it
            const int maxMillisecondsTimeout = 100;
            DistributedLockTimeToLive timeToLive = TimeSpan.FromMilliseconds(new Random().Next(1, maxMillisecondsTimeout));
            var success = await DistributedLockRepository.TryInsert(
                resourceId,
                lockId,
                timeToLive,
                CancellationToken.None);
            Assert.True(success);

            await Task.Delay(maxMillisecondsTimeout);
            var result = await DistributedLockRepository.TryUpdateTimeToLiveAsync(
                resourceId,
                lockId,
                timeToLive,
                CancellationToken.None);
            
            Assert.False(result);
        }

        [Theory]
        [AutoMoqData]
        public async Task FailToExtendAcquiredLock_When_TryingToExtendLock_And_LockIdIsCorrectButResourceIdIsNot(
            DistributedLockResourceId resourceId,
            DistributedLockId lockId,
            DistributedLockResourceId anotherResourceId)
        {
            // make sure the timeout will last no longer than until we try to release it
            const int maxMillisecondsTimeout = 100;
            DistributedLockTimeToLive timeToLive = TimeSpan.FromMilliseconds(new Random().Next(1, maxMillisecondsTimeout));
            var success = await DistributedLockRepository.TryInsert(
                resourceId,
                lockId,
                timeToLive,
                CancellationToken.None);
            Assert.True(success);

            await Task.Delay(maxMillisecondsTimeout);
            var result = await DistributedLockRepository.TryUpdateTimeToLiveAsync(
                anotherResourceId,
                lockId,
                timeToLive,
                CancellationToken.None);
            
            Assert.False(result);
        }

        [Theory]
        [AutoMoqData]
        public async Task FailToExtendAcquiredLock_When_TryingToExtendLock_And_LockIdIsIncorrectButResourceIdIsCorrect(
            DistributedLockResourceId resourceId,
            DistributedLockId lockId,
            DistributedLockId anotherId)
        {
            // make sure the timeout will last no longer than until we try to release it
            const int maxMillisecondsTimeout = 100;
            DistributedLockTimeToLive timeToLive = TimeSpan.FromMilliseconds(new Random().Next(1, maxMillisecondsTimeout));
            var success = await DistributedLockRepository.TryInsert(
                resourceId,
                lockId,
                timeToLive,
                CancellationToken.None);
            Assert.True(success);

            await Task.Delay(maxMillisecondsTimeout);
            var result = await DistributedLockRepository.TryUpdateTimeToLiveAsync(
                resourceId,
                anotherId,
                timeToLive,
                CancellationToken.None);
            
            Assert.False(result);
        }

        [Theory]
        [AutoMoqData]
        public async Task FailToExtendAcquiredLock_When_TryingToExtendLock_And_LockWasNotAcquired(
            DistributedLockResourceId resourceId,
            DistributedLockId id,
            DistributedLockTimeToLive timeToLive)
        {
            var result = await DistributedLockRepository.TryUpdateTimeToLiveAsync(
                resourceId,
                id,
                timeToLive,
                CancellationToken.None);
            
            Assert.False(result);
        }
    }
}