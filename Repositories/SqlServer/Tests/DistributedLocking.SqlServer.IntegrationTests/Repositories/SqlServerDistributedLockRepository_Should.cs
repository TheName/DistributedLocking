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
            DistributedLockIdentifier identifier,
            DistributedLockId lockId,
            DistributedLockTimeToLive timeToLive)
        {
            var success = await DistributedLockRepository.TryInsert(
                identifier,
                lockId,
                timeToLive,
                CancellationToken.None);
            
            Assert.True(success);
        }

        [Theory]
        [AutoMoqData]
        public async Task FailToAcquireLock_When_TryingToAcquire_And_IdentifierIsAlreadyAcquired(
            DistributedLockIdentifier identifier,
            DistributedLockId lockId,
            DistributedLockTimeToLive timeToLive)
        {
            // make sure the timeout will last at least until we try to acquire again
            timeToLive = new DistributedLockTimeToLive(timeToLive.Value + TimeSpan.FromSeconds(5));
            var success = await DistributedLockRepository.TryInsert(
                identifier,
                lockId,
                timeToLive,
                CancellationToken.None);
            Assert.True(success);
            
            success = await DistributedLockRepository.TryInsert(
                identifier,
                lockId,
                timeToLive,
                CancellationToken.None);
            Assert.False(success);
        }

        [Theory]
        [AutoMoqData]
        public async Task AcquireLockOnlyOnOneThread_When_TryingToAcquireLockInParallel(
            DistributedLockIdentifier identifier,
            DistributedLockId lockId,
            DistributedLockTimeToLive timeToLive)
        {
            // make sure the timeout will last at least until all tasks are done
            timeToLive = new DistributedLockTimeToLive(timeToLive.Value + TimeSpan.FromSeconds(5));
            var tryAcquireTasks = Enumerable.Range(0, 1000)
                .Select(i =>
                    DistributedLockRepository.TryInsert(identifier, lockId, timeToLive, CancellationToken.None))
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
            DistributedLockIdentifier identifier,
            DistributedLockId lockId,
            DistributedLockTimeToLive timeToLive)
        {
            // make sure the timeout will last at least until the release is called
            timeToLive = new DistributedLockTimeToLive(timeToLive.Value + TimeSpan.FromSeconds(5));
            var success = await DistributedLockRepository.TryInsert(
                identifier,
                lockId,
                timeToLive,
                CancellationToken.None);
            Assert.True(success);

            var result = await DistributedLockRepository.TryDelete(identifier, lockId, CancellationToken.None);
            
            Assert.True(result);
        }

        [Theory]
        [AutoMoqData]
        public async Task FailToReleaseAcquiredLock_When_TryingToReleaseLock_And_TimeoutHasExpired(
            DistributedLockIdentifier identifier,
            DistributedLockId lockId,
            DistributedLockTimeToLive timeToLive)
        {
            // make sure the timeout will last no longer than until we try to release it
            const int maxMillisecondsTimeout = 100;
            timeToLive = new DistributedLockTimeToLive(TimeSpan.FromMilliseconds(timeToLive.Value.TotalMilliseconds % maxMillisecondsTimeout));
            var success = await DistributedLockRepository.TryInsert(
                identifier,
                lockId,
                timeToLive,
                CancellationToken.None);
            Assert.True(success);

            await Task.Delay(maxMillisecondsTimeout);
            var result = await DistributedLockRepository.TryDelete(identifier, lockId, CancellationToken.None);
            
            Assert.False(result);
        }

        [Theory]
        [AutoMoqData]
        public async Task FailToReleaseAcquiredLock_When_TryingToReleaseLock_And_LockWasNotAcquired(
            DistributedLockIdentifier identifier,
            DistributedLockId id)
        {
            var result = await DistributedLockRepository.TryDelete(identifier, id, CancellationToken.None);
            
            Assert.False(result);
        }

        [Theory]
        [AutoMoqData]
        public async Task AcquireLock_When_TryingToAcquireLock_And_LockWasAcquiredAndAlreadyReleased(
            DistributedLockIdentifier identifier,
            DistributedLockId lockId,
            DistributedLockTimeToLive timeToLive)
        {
            // make sure the timeout will last at least until the release is called
            timeToLive = new DistributedLockTimeToLive(timeToLive.Value + TimeSpan.FromSeconds(5));
            var success = await DistributedLockRepository.TryInsert(
                identifier,
                lockId,
                timeToLive,
                CancellationToken.None);
            Assert.True(success);
            var result = await DistributedLockRepository.TryDelete(identifier, lockId, CancellationToken.None);
            Assert.True(result);
            
            success = await DistributedLockRepository.TryInsert(
                identifier,
                lockId,
                timeToLive,
                CancellationToken.None);
            Assert.True(success);
        }

        [Theory]
        [AutoMoqData]
        public async Task FailToReleaseAcquiredLock_When_TryingToReleaseLock_And_IdIsCorrectButIdentifierIsNot(
            DistributedLockIdentifier identifier,
            DistributedLockId lockId,
            DistributedLockIdentifier anotherIdentifier,
            DistributedLockTimeToLive timeToLive)
        {
            // make sure the timeout will last no longer than until we try to release it
            const int maxMillisecondsTimeout = 100;
            timeToLive = new DistributedLockTimeToLive(TimeSpan.FromMilliseconds(timeToLive.Value.TotalMilliseconds % maxMillisecondsTimeout));
            
            var success = await DistributedLockRepository.TryInsert(
                identifier,
                lockId,
                timeToLive,
                CancellationToken.None);
            Assert.True(success);

            await Task.Delay(maxMillisecondsTimeout);
            var result = await DistributedLockRepository.TryDelete(
                anotherIdentifier,
                lockId,
                CancellationToken.None);
            
            Assert.False(result);
        }

        [Theory]
        [AutoMoqData]
        public async Task FailToReleaseAcquiredLock_When_TryingToReleaseLock_And_IdIsIncorrectButIdentifierIsCorrect(
            DistributedLockIdentifier identifier,
            DistributedLockId lockId,
            DistributedLockId anotherId,
            DistributedLockTimeToLive timeToLive)
        {
            // make sure the timeout will last no longer than until we try to release it
            const int maxMillisecondsTimeout = 100;
            timeToLive = new DistributedLockTimeToLive(TimeSpan.FromMilliseconds(timeToLive.Value.TotalMilliseconds % maxMillisecondsTimeout));
            var success = await DistributedLockRepository.TryInsert(
                identifier,
                lockId,
                timeToLive,
                CancellationToken.None);
            Assert.True(success);

            await Task.Delay(maxMillisecondsTimeout);
            var result = await DistributedLockRepository.TryDelete(
                identifier,
                anotherId,
                CancellationToken.None);
            
            Assert.False(result);
        }

        [Theory]
        [AutoMoqData]
        public async Task ExtendAcquiredLock(
            DistributedLockIdentifier identifier,
            DistributedLockId lockId,
            DistributedLockTimeToLive timeToLive)
        {
            // make sure the timeout will last at least until the release is called
            timeToLive = new DistributedLockTimeToLive(timeToLive.Value + TimeSpan.FromSeconds(5));
            var success = await DistributedLockRepository.TryInsert(
                identifier,
                lockId,
                timeToLive,
                CancellationToken.None);
            Assert.True(success);

            var result = await DistributedLockRepository.TryUpdateTimeToLiveAsync(
                identifier,
                lockId,
                timeToLive,
                CancellationToken.None);
            
            Assert.True(result);
        }

        [Theory]
        [AutoMoqData]
        public async Task FailToExtendAcquiredLock_When_TryingToExtendLock_And_TimeoutHasExpired(
            DistributedLockIdentifier identifier,
            DistributedLockId lockId,
            DistributedLockTimeToLive timeToLive)
        {
            // make sure the timeout will last no longer than until we try to release it
            const int maxMillisecondsTimeout = 100;
            timeToLive = new DistributedLockTimeToLive(TimeSpan.FromMilliseconds(timeToLive.Value.TotalMilliseconds % maxMillisecondsTimeout));
            var success = await DistributedLockRepository.TryInsert(
                identifier,
                lockId,
                timeToLive,
                CancellationToken.None);
            Assert.True(success);

            await Task.Delay(maxMillisecondsTimeout);
            var result = await DistributedLockRepository.TryUpdateTimeToLiveAsync(
                identifier,
                lockId,
                timeToLive,
                CancellationToken.None);
            
            Assert.False(result);
        }

        [Theory]
        [AutoMoqData]
        public async Task FailToExtendAcquiredLock_When_TryingToExtendLock_And_IdIsCorrectButIdentifierIsNot(
            DistributedLockIdentifier identifier,
            DistributedLockId lockId,
            DistributedLockIdentifier anotherIdentifier,
            DistributedLockTimeToLive timeToLive)
        {
            // make sure the timeout will last no longer than until we try to release it
            const int maxMillisecondsTimeout = 100;
            timeToLive = new DistributedLockTimeToLive(TimeSpan.FromMilliseconds(timeToLive.Value.TotalMilliseconds % maxMillisecondsTimeout));
            var success = await DistributedLockRepository.TryInsert(
                identifier,
                lockId,
                timeToLive,
                CancellationToken.None);
            Assert.True(success);

            await Task.Delay(maxMillisecondsTimeout);
            var result = await DistributedLockRepository.TryUpdateTimeToLiveAsync(
                anotherIdentifier,
                lockId,
                timeToLive,
                CancellationToken.None);
            
            Assert.False(result);
        }

        [Theory]
        [AutoMoqData]
        public async Task FailToExtendAcquiredLock_When_TryingToExtendLock_And_IdIsIncorrectButIdentifierIsCorrect(
            DistributedLockIdentifier identifier,
            DistributedLockId lockId,
            DistributedLockId anotherId,
            DistributedLockTimeToLive timeToLive)
        {
            // make sure the timeout will last no longer than until we try to release it
            const int maxMillisecondsTimeout = 100;
            timeToLive = new DistributedLockTimeToLive(TimeSpan.FromMilliseconds(timeToLive.Value.TotalMilliseconds % maxMillisecondsTimeout));
            var success = await DistributedLockRepository.TryInsert(
                identifier,
                lockId,
                timeToLive,
                CancellationToken.None);
            Assert.True(success);

            await Task.Delay(maxMillisecondsTimeout);
            var result = await DistributedLockRepository.TryUpdateTimeToLiveAsync(
                identifier,
                anotherId,
                timeToLive,
                CancellationToken.None);
            
            Assert.False(result);
        }

        [Theory]
        [AutoMoqData]
        public async Task FailToExtendAcquiredLock_When_TryingToExtendLock_And_LockWasNotAcquired(
            DistributedLockIdentifier identifier,
            DistributedLockId id,
            DistributedLockTimeToLive timeToLive)
        {
            var result = await DistributedLockRepository.TryUpdateTimeToLiveAsync(
                identifier,
                id,
                timeToLive,
                CancellationToken.None);
            
            Assert.False(result);
        }
    }
}