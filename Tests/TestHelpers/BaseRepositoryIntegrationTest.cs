using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.Abstractions;
using DistributedLocking.Abstractions.Repositories;
using TestHelpers.Attributes;
using Xunit;

namespace TestHelpers
{
    public abstract class BaseRepositoryIntegrationTest
    {
        protected abstract IDistributedLocksRepository DistributedLockRepository { get; }
        
        [Theory]
        [AutoMoqData]
        public async Task AcquireLock(
            DistributedLockResourceId resourceId,
            DistributedLockId lockId,
            uint seconds)
        {
            var success = await DistributedLockRepository.TryInsert(
                resourceId,
                lockId,
                TimeSpan.FromSeconds(seconds),
                CancellationToken.None);
            
            Assert.True(success);
        }

        [Theory]
        [AutoMoqWithInlineData(900)]
        public async Task AcquireLockWithLongResourceId(
            int numberOfCharacters,
            char c,
            DistributedLockId lockId,
            uint seconds)
        {
            var resourceId = new string(c, numberOfCharacters);
            
            var success = await DistributedLockRepository.TryInsert(
                resourceId,
                lockId,
                TimeSpan.FromSeconds(seconds),
                CancellationToken.None);
            
            Assert.True(success);
        }

        [Theory]
        [AutoMoqData]
        public async Task FailToAcquireLock_When_TryingToAcquire_And_ResourceIdIsAlreadyAcquired(
            DistributedLockResourceId resourceId,
            DistributedLockId lockId)
        {
            // make sure the timeout will last at least until we try to acquire again
            var timeToLive = TimeSpan.FromSeconds(5);
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
            var tryAcquireTasks = Enumerable.Range(0, 50)
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
            const int maxSecondsTimeout = 10;
            DistributedLockTimeToLive timeToLive = TimeSpan.FromSeconds(new Random().Next(2, maxSecondsTimeout));
            var success = await DistributedLockRepository.TryInsert(
                resourceId,
                lockId,
                timeToLive,
                CancellationToken.None);
            Assert.True(success);

            await Task.Delay(TimeSpan.FromSeconds(maxSecondsTimeout));
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
            const int maxSecondsTimeout = 3;
            DistributedLockTimeToLive timeToLive = TimeSpan.FromSeconds(new Random().Next(1, maxSecondsTimeout));
            
            var success = await DistributedLockRepository.TryInsert(
                resourceId,
                lockId,
                timeToLive,
                CancellationToken.None);
            Assert.True(success);

            await Task.Delay(maxSecondsTimeout);
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
            const int maxSecondsTimeout = 3;
            DistributedLockTimeToLive timeToLive = TimeSpan.FromSeconds(new Random().Next(1, maxSecondsTimeout));
            var success = await DistributedLockRepository.TryInsert(
                resourceId,
                lockId,
                timeToLive,
                CancellationToken.None);
            Assert.True(success);

            await Task.Delay(maxSecondsTimeout);
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
            const int maxSecondsTimeout = 10;
            DistributedLockTimeToLive timeToLive = TimeSpan.FromSeconds(new Random().Next(2, maxSecondsTimeout));
            var success = await DistributedLockRepository.TryInsert(
                resourceId,
                lockId,
                timeToLive,
                CancellationToken.None);
            Assert.True(success);

            await Task.Delay(TimeSpan.FromSeconds(maxSecondsTimeout));
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
            const int maxSecondsTimeout = 3;
            DistributedLockTimeToLive timeToLive = TimeSpan.FromSeconds(new Random().Next(1, maxSecondsTimeout));
            var success = await DistributedLockRepository.TryInsert(
                resourceId,
                lockId,
                timeToLive,
                CancellationToken.None);
            Assert.True(success);

            await Task.Delay(maxSecondsTimeout);
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
            const int maxSecondsTimeout = 3;
            DistributedLockTimeToLive timeToLive = TimeSpan.FromSeconds(new Random().Next(1, maxSecondsTimeout));
            var success = await DistributedLockRepository.TryInsert(
                resourceId,
                lockId,
                timeToLive,
                CancellationToken.None);
            Assert.True(success);

            await Task.Delay(maxSecondsTimeout);
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