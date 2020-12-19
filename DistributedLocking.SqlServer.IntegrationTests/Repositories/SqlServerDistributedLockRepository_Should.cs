using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.SqlServer.IntegrationTests.CollectionDefinitions;
using DistributedLocking.SqlServer.IntegrationTests.Fixtures;
using TestHelpers.Attributes;
using TheName.DistributedLocking.Abstractions.Records;
using TheName.DistributedLocking.Abstractions.Repositories;
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
            LockTimeout lockTimeout)
        {
            var (success, acquiredLockId) = await DistributedLockRepository.TryAcquireAsync(lockIdentifier, lockTimeout, CancellationToken.None);
            
            Assert.True(success);
            Assert.NotEqual(Guid.Empty, acquiredLockId.Value);
        }

        [Theory]
        [AutoMoqData]
        public async Task FailToAcquireLock_When_TryingToAcquire_And_LockIdentifierIsAlreadyAcquired(
            DistributedLockIdentifier lockIdentifier,
            LockTimeout lockTimeout)
        {
            // make sure the timeout will last at least until we try to acquire again
            lockTimeout = new LockTimeout(lockTimeout.Value + TimeSpan.FromSeconds(5));
            var (success, _) = await DistributedLockRepository.TryAcquireAsync(lockIdentifier, lockTimeout, CancellationToken.None);
            Assert.True(success);
            
            (success, _) = await DistributedLockRepository.TryAcquireAsync(lockIdentifier, lockTimeout, CancellationToken.None);
            Assert.False(success);
        }

        [Theory]
        [AutoMoqData]
        public async Task AcquireLockOnlyOnOneThread_When_TryingToAcquireLockInParallel(
            DistributedLockIdentifier lockIdentifier,
            LockTimeout lockTimeout)
        {
            // make sure the timeout will last at least until all tasks are done
            lockTimeout = new LockTimeout(lockTimeout.Value + TimeSpan.FromSeconds(5));
            var tryAcquireTasks = Enumerable.Range(0, 1000)
                .Select(i =>
                    DistributedLockRepository.TryAcquireAsync(lockIdentifier, lockTimeout, CancellationToken.None))
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
            LockTimeout lockTimeout)
        {
            // make sure the timeout will last at least until the release is called
            lockTimeout = new LockTimeout(lockTimeout.Value + TimeSpan.FromSeconds(5));
            var (success, acquiredLockId) = await DistributedLockRepository.TryAcquireAsync(lockIdentifier, lockTimeout, CancellationToken.None);
            Assert.True(success);

            var result = await DistributedLockRepository.TryReleaseAsync(acquiredLockId, CancellationToken.None);
            
            Assert.True(result);
        }

        [Theory]
        [AutoMoqData]
        public async Task FailToReleaseAcquiredLock_When_TryingToReleaseLock_And_TimeoutHasExpired(
            DistributedLockIdentifier lockIdentifier,
            LockTimeout lockTimeout)
        {
            // make sure the timeout will last no longer than until we try to release it
            const int maxMillisecondsTimeout = 100;
            lockTimeout = new LockTimeout(TimeSpan.FromMilliseconds(lockTimeout.Value.TotalMilliseconds % maxMillisecondsTimeout));
            var (success, acquiredLockId) = await DistributedLockRepository.TryAcquireAsync(lockIdentifier, lockTimeout, CancellationToken.None);
            Assert.True(success);

            await Task.Delay(maxMillisecondsTimeout);
            var result = await DistributedLockRepository.TryReleaseAsync(acquiredLockId, CancellationToken.None);
            
            Assert.False(result);
        }

        [Theory]
        [AutoMoqData]
        public async Task FailToReleaseAcquiredLock_When_TryingToReleaseLock_And_LockWasNotAcquired(
            DistributedLockId lockId)
        {
            var result = await DistributedLockRepository.TryReleaseAsync(lockId, CancellationToken.None);
            
            Assert.False(result);
        }

        [Theory]
        [AutoMoqData]
        public async Task AcquireLock_When_TryingToAcquireLock_And_LockWasAcquiredAndAlreadyReleased(
            DistributedLockIdentifier lockIdentifier,
            LockTimeout lockTimeout)
        {
            // make sure the timeout will last at least until the release is called
            lockTimeout = new LockTimeout(lockTimeout.Value + TimeSpan.FromSeconds(5));
            var (success, acquiredLockId) = await DistributedLockRepository.TryAcquireAsync(lockIdentifier, lockTimeout, CancellationToken.None);
            Assert.True(success);
            var result = await DistributedLockRepository.TryReleaseAsync(acquiredLockId, CancellationToken.None);
            Assert.True(result);

            (success, _) = await DistributedLockRepository.TryAcquireAsync(lockIdentifier, lockTimeout, CancellationToken.None);
            Assert.True(success);
        }
    }
}