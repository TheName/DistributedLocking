using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.Abstractions.Records;
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

        private IDistributedLockRepository DistributedLockRepository =>
            _sqlDatabaseFixture.GetService<IDistributedLockRepository>();

        public SqlServerDistributedLockRepository_Should(SqlDatabaseFixture sqlDatabaseFixture)
        {
            _sqlDatabaseFixture = sqlDatabaseFixture ?? throw new ArgumentNullException(nameof(sqlDatabaseFixture));
        }

        [Theory]
        [AutoMoqData]
        public async Task AcquireLock(
            DistributedLockIdentifier lockIdentifier,
            DistributedLockTimeToLive lockTimeToLive)
        {
            var (success, acquiredLockId) = await DistributedLockRepository.TryAcquireAsync(lockIdentifier, lockTimeToLive, CancellationToken.None);
            
            Assert.True(success);
            Assert.NotEqual(Guid.Empty, acquiredLockId.Value);
        }

        [Theory]
        [AutoMoqData]
        public async Task FailToAcquireLock_When_TryingToAcquire_And_LockIdentifierIsAlreadyAcquired(
            DistributedLockIdentifier lockIdentifier,
            DistributedLockTimeToLive lockTimeToLive)
        {
            // make sure the timeout will last at least until we try to acquire again
            lockTimeToLive = new DistributedLockTimeToLive(lockTimeToLive.Value + TimeSpan.FromSeconds(5));
            var (success, _) = await DistributedLockRepository.TryAcquireAsync(lockIdentifier, lockTimeToLive, CancellationToken.None);
            Assert.True(success);
            
            (success, _) = await DistributedLockRepository.TryAcquireAsync(lockIdentifier, lockTimeToLive, CancellationToken.None);
            Assert.False(success);
        }

        [Theory]
        [AutoMoqData]
        public async Task AcquireLockOnlyOnOneThread_When_TryingToAcquireLockInParallel(
            DistributedLockIdentifier lockIdentifier,
            DistributedLockTimeToLive lockTimeToLive)
        {
            // make sure the timeout will last at least until all tasks are done
            lockTimeToLive = new DistributedLockTimeToLive(lockTimeToLive.Value + TimeSpan.FromSeconds(5));
            var tryAcquireTasks = Enumerable.Range(0, 1000)
                .Select(i =>
                    DistributedLockRepository.TryAcquireAsync(lockIdentifier, lockTimeToLive, CancellationToken.None))
                .ToList();

            await Task.WhenAll(tryAcquireTasks);

            var tryAcquireResults = tryAcquireTasks
                .Select(task => task.Result)
                .ToList();

            Assert.Single(tryAcquireResults, tuple => tuple.Success);
        }

        [Theory]
        [AutoMoqData]
        public async Task ReleaseAcquiredLock(
            DistributedLockIdentifier lockIdentifier,
            DistributedLockTimeToLive lockTimeToLive)
        {
            // make sure the timeout will last at least until the release is called
            lockTimeToLive = new DistributedLockTimeToLive(lockTimeToLive.Value + TimeSpan.FromSeconds(5));
            var (success, acquiredLockId) = await DistributedLockRepository.TryAcquireAsync(lockIdentifier, lockTimeToLive, CancellationToken.None);
            Assert.True(success);

            var result = await DistributedLockRepository.TryReleaseAsync(lockIdentifier, acquiredLockId, CancellationToken.None);
            
            Assert.True(result);
        }

        [Theory]
        [AutoMoqData]
        public async Task FailToReleaseAcquiredLock_When_TryingToReleaseLock_And_TimeoutHasExpired(
            DistributedLockIdentifier lockIdentifier,
            DistributedLockTimeToLive lockTimeToLive)
        {
            // make sure the timeout will last no longer than until we try to release it
            const int maxMillisecondsTimeout = 100;
            lockTimeToLive = new DistributedLockTimeToLive(TimeSpan.FromMilliseconds(lockTimeToLive.Value.TotalMilliseconds % maxMillisecondsTimeout));
            var (success, acquiredLockId) = await DistributedLockRepository.TryAcquireAsync(lockIdentifier, lockTimeToLive, CancellationToken.None);
            Assert.True(success);

            await Task.Delay(maxMillisecondsTimeout);
            var result = await DistributedLockRepository.TryReleaseAsync(lockIdentifier, acquiredLockId, CancellationToken.None);
            
            Assert.False(result);
        }

        [Theory]
        [AutoMoqData]
        public async Task FailToReleaseAcquiredLock_When_TryingToReleaseLock_And_LockWasNotAcquired(
            DistributedLockIdentifier lockIdentifier,
            DistributedLockId lockId)
        {
            var result = await DistributedLockRepository.TryReleaseAsync(lockIdentifier, lockId, CancellationToken.None);
            
            Assert.False(result);
        }

        [Theory]
        [AutoMoqData]
        public async Task AcquireLock_When_TryingToAcquireLock_And_LockWasAcquiredAndAlreadyReleased(
            DistributedLockIdentifier lockIdentifier,
            DistributedLockTimeToLive lockTimeToLive)
        {
            // make sure the timeout will last at least until the release is called
            lockTimeToLive = new DistributedLockTimeToLive(lockTimeToLive.Value + TimeSpan.FromSeconds(5));
            var (success, acquiredLockId) = await DistributedLockRepository.TryAcquireAsync(lockIdentifier, lockTimeToLive, CancellationToken.None);
            Assert.True(success);
            var result = await DistributedLockRepository.TryReleaseAsync(lockIdentifier, acquiredLockId, CancellationToken.None);
            Assert.True(result);

            (success, _) = await DistributedLockRepository.TryAcquireAsync(lockIdentifier, lockTimeToLive, CancellationToken.None);
            Assert.True(success);
        }

        [Theory]
        [AutoMoqData]
        public async Task FailToReleaseAcquiredLock_When_TryingToReleaseLock_And_LockIdIsCorrectButLockIdentifierIsNot(
            DistributedLockIdentifier lockIdentifier,
            DistributedLockIdentifier anotherLockIdentifier,
            DistributedLockTimeToLive lockTimeToLive)
        {
            // make sure the timeout will last no longer than until we try to release it
            const int maxMillisecondsTimeout = 100;
            lockTimeToLive = new DistributedLockTimeToLive(TimeSpan.FromMilliseconds(lockTimeToLive.Value.TotalMilliseconds % maxMillisecondsTimeout));
            var (success, acquiredLockId) = await DistributedLockRepository.TryAcquireAsync(lockIdentifier, lockTimeToLive, CancellationToken.None);
            Assert.True(success);

            await Task.Delay(maxMillisecondsTimeout);
            var result = await DistributedLockRepository.TryReleaseAsync(
                anotherLockIdentifier,
                acquiredLockId,
                CancellationToken.None);
            
            Assert.False(result);
        }

        [Theory]
        [AutoMoqData]
        public async Task FailToReleaseAcquiredLock_When_TryingToReleaseLock_And_LockIdIsIncorrectButLockIdentifierIs(
            DistributedLockIdentifier lockIdentifier,
            DistributedLockId anotherLockId,
            DistributedLockTimeToLive lockTimeToLive)
        {
            // make sure the timeout will last no longer than until we try to release it
            const int maxMillisecondsTimeout = 100;
            lockTimeToLive = new DistributedLockTimeToLive(TimeSpan.FromMilliseconds(lockTimeToLive.Value.TotalMilliseconds % maxMillisecondsTimeout));
            var (success, _) = await DistributedLockRepository.TryAcquireAsync(lockIdentifier, lockTimeToLive, CancellationToken.None);
            Assert.True(success);

            await Task.Delay(maxMillisecondsTimeout);
            var result = await DistributedLockRepository.TryReleaseAsync(
                lockIdentifier,
                anotherLockId,
                CancellationToken.None);
            
            Assert.False(result);
        }

        [Theory]
        [AutoMoqData]
        public async Task ExtendAcquiredLock(
            DistributedLockIdentifier lockIdentifier,
            DistributedLockTimeToLive lockTimeToLive)
        {
            // make sure the timeout will last at least until the release is called
            lockTimeToLive = new DistributedLockTimeToLive(lockTimeToLive.Value + TimeSpan.FromSeconds(5));
            var (success, acquiredLockId) = await DistributedLockRepository.TryAcquireAsync(lockIdentifier, lockTimeToLive, CancellationToken.None);
            Assert.True(success);

            var result = await DistributedLockRepository.TryExtendAsync(
                lockIdentifier,
                acquiredLockId,
                lockTimeToLive,
                CancellationToken.None);
            
            Assert.True(result);
        }

        [Theory]
        [AutoMoqData]
        public async Task FailToExtendAcquiredLock_When_TryingToExtendLock_And_TimeoutHasExpired(
            DistributedLockIdentifier lockIdentifier,
            DistributedLockTimeToLive lockTimeToLive)
        {
            // make sure the timeout will last no longer than until we try to release it
            const int maxMillisecondsTimeout = 100;
            lockTimeToLive = new DistributedLockTimeToLive(TimeSpan.FromMilliseconds(lockTimeToLive.Value.TotalMilliseconds % maxMillisecondsTimeout));
            var (success, acquiredLockId) = await DistributedLockRepository.TryAcquireAsync(lockIdentifier, lockTimeToLive, CancellationToken.None);
            Assert.True(success);

            await Task.Delay(maxMillisecondsTimeout);
            var result = await DistributedLockRepository.TryExtendAsync(
                lockIdentifier,
                acquiredLockId,
                lockTimeToLive,
                CancellationToken.None);
            
            Assert.False(result);
        }

        [Theory]
        [AutoMoqData]
        public async Task FailToExtendAcquiredLock_When_TryingToExtendLock_And_LockIdIsCorrectButLockIdentifierIsNot(
            DistributedLockIdentifier lockIdentifier,
            DistributedLockIdentifier anotherLockIdentifier,
            DistributedLockTimeToLive lockTimeToLive)
        {
            // make sure the timeout will last no longer than until we try to release it
            const int maxMillisecondsTimeout = 100;
            lockTimeToLive = new DistributedLockTimeToLive(TimeSpan.FromMilliseconds(lockTimeToLive.Value.TotalMilliseconds % maxMillisecondsTimeout));
            var (success, acquiredLockId) = await DistributedLockRepository.TryAcquireAsync(lockIdentifier, lockTimeToLive, CancellationToken.None);
            Assert.True(success);

            await Task.Delay(maxMillisecondsTimeout);
            var result = await DistributedLockRepository.TryExtendAsync(
                anotherLockIdentifier,
                acquiredLockId,
                lockTimeToLive,
                CancellationToken.None);
            
            Assert.False(result);
        }

        [Theory]
        [AutoMoqData]
        public async Task FailToExtendAcquiredLock_When_TryingToExtendLock_And_LockIdIsIncorrectButLockIdentifierIs(
            DistributedLockIdentifier lockIdentifier,
            DistributedLockId anotherLockId,
            DistributedLockTimeToLive lockTimeToLive)
        {
            // make sure the timeout will last no longer than until we try to release it
            const int maxMillisecondsTimeout = 100;
            lockTimeToLive = new DistributedLockTimeToLive(TimeSpan.FromMilliseconds(lockTimeToLive.Value.TotalMilliseconds % maxMillisecondsTimeout));
            var (success, _) = await DistributedLockRepository.TryAcquireAsync(lockIdentifier, lockTimeToLive, CancellationToken.None);
            Assert.True(success);

            await Task.Delay(maxMillisecondsTimeout);
            var result = await DistributedLockRepository.TryExtendAsync(
                lockIdentifier,
                anotherLockId,
                lockTimeToLive,
                CancellationToken.None);
            
            Assert.False(result);
        }

        [Theory]
        [AutoMoqData]
        public async Task FailToExtendAcquiredLock_When_TryingToExtendLock_And_LockWasNotAcquired(
            DistributedLockIdentifier lockIdentifier,
            DistributedLockId lockId,
            DistributedLockTimeToLive lockTimeToLive)
        {
            var result = await DistributedLockRepository.TryExtendAsync(
                lockIdentifier,
                lockId,
                lockTimeToLive,
                CancellationToken.None);
            
            Assert.False(result);
        }
    }
}