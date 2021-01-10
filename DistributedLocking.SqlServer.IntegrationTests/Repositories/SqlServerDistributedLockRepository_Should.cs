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
            DistributedLockIdentifier identifier,
            DistributedLockTimeToLive timeToLive)
        {
            var (success, acquiredLockId) = await DistributedLockRepository.TryAcquireAsync(identifier, timeToLive, CancellationToken.None);
            
            Assert.True(success);
            Assert.NotEqual(Guid.Empty, acquiredLockId.Value);
        }

        [Theory]
        [AutoMoqData]
        public async Task FailToAcquireLock_When_TryingToAcquire_And_IdentifierIsAlreadyAcquired(
            DistributedLockIdentifier identifier,
            DistributedLockTimeToLive timeToLive)
        {
            // make sure the timeout will last at least until we try to acquire again
            timeToLive = new DistributedLockTimeToLive(timeToLive.Value + TimeSpan.FromSeconds(5));
            var (success, _) = await DistributedLockRepository.TryAcquireAsync(identifier, timeToLive, CancellationToken.None);
            Assert.True(success);
            
            (success, _) = await DistributedLockRepository.TryAcquireAsync(identifier, timeToLive, CancellationToken.None);
            Assert.False(success);
        }

        [Theory]
        [AutoMoqData]
        public async Task AcquireLockOnlyOnOneThread_When_TryingToAcquireLockInParallel(
            DistributedLockIdentifier identifier,
            DistributedLockTimeToLive timeToLive)
        {
            // make sure the timeout will last at least until all tasks are done
            timeToLive = new DistributedLockTimeToLive(timeToLive.Value + TimeSpan.FromSeconds(5));
            var tryAcquireTasks = Enumerable.Range(0, 1000)
                .Select(i =>
                    DistributedLockRepository.TryAcquireAsync(identifier, timeToLive, CancellationToken.None))
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
            DistributedLockIdentifier identifier,
            DistributedLockTimeToLive timeToLive)
        {
            // make sure the timeout will last at least until the release is called
            timeToLive = new DistributedLockTimeToLive(timeToLive.Value + TimeSpan.FromSeconds(5));
            var (success, acquiredLockId) = await DistributedLockRepository.TryAcquireAsync(identifier, timeToLive, CancellationToken.None);
            Assert.True(success);

            var result = await DistributedLockRepository.TryReleaseAsync(identifier, acquiredLockId, CancellationToken.None);
            
            Assert.True(result);
        }

        [Theory]
        [AutoMoqData]
        public async Task FailToReleaseAcquiredLock_When_TryingToReleaseLock_And_TimeoutHasExpired(
            DistributedLockIdentifier identifier,
            DistributedLockTimeToLive timeToLive)
        {
            // make sure the timeout will last no longer than until we try to release it
            const int maxMillisecondsTimeout = 100;
            timeToLive = new DistributedLockTimeToLive(TimeSpan.FromMilliseconds(timeToLive.Value.TotalMilliseconds % maxMillisecondsTimeout));
            var (success, acquiredLockId) = await DistributedLockRepository.TryAcquireAsync(identifier, timeToLive, CancellationToken.None);
            Assert.True(success);

            await Task.Delay(maxMillisecondsTimeout);
            var result = await DistributedLockRepository.TryReleaseAsync(identifier, acquiredLockId, CancellationToken.None);
            
            Assert.False(result);
        }

        [Theory]
        [AutoMoqData]
        public async Task FailToReleaseAcquiredLock_When_TryingToReleaseLock_And_LockWasNotAcquired(
            DistributedLockIdentifier identifier,
            DistributedLockId id)
        {
            var result = await DistributedLockRepository.TryReleaseAsync(identifier, id, CancellationToken.None);
            
            Assert.False(result);
        }

        [Theory]
        [AutoMoqData]
        public async Task AcquireLock_When_TryingToAcquireLock_And_LockWasAcquiredAndAlreadyReleased(
            DistributedLockIdentifier identifier,
            DistributedLockTimeToLive timeToLive)
        {
            // make sure the timeout will last at least until the release is called
            timeToLive = new DistributedLockTimeToLive(timeToLive.Value + TimeSpan.FromSeconds(5));
            var (success, acquiredLockId) = await DistributedLockRepository.TryAcquireAsync(identifier, timeToLive, CancellationToken.None);
            Assert.True(success);
            var result = await DistributedLockRepository.TryReleaseAsync(identifier, acquiredLockId, CancellationToken.None);
            Assert.True(result);

            (success, _) = await DistributedLockRepository.TryAcquireAsync(identifier, timeToLive, CancellationToken.None);
            Assert.True(success);
        }

        [Theory]
        [AutoMoqData]
        public async Task FailToReleaseAcquiredLock_When_TryingToReleaseLock_And_IdIsCorrectButIdentifierIsNot(
            DistributedLockIdentifier identifier,
            DistributedLockIdentifier anotherIdentifier,
            DistributedLockTimeToLive timeToLive)
        {
            // make sure the timeout will last no longer than until we try to release it
            const int maxMillisecondsTimeout = 100;
            timeToLive = new DistributedLockTimeToLive(TimeSpan.FromMilliseconds(timeToLive.Value.TotalMilliseconds % maxMillisecondsTimeout));
            var (success, acquiredLockId) = await DistributedLockRepository.TryAcquireAsync(identifier, timeToLive, CancellationToken.None);
            Assert.True(success);

            await Task.Delay(maxMillisecondsTimeout);
            var result = await DistributedLockRepository.TryReleaseAsync(
                anotherIdentifier,
                acquiredLockId,
                CancellationToken.None);
            
            Assert.False(result);
        }

        [Theory]
        [AutoMoqData]
        public async Task FailToReleaseAcquiredLock_When_TryingToReleaseLock_And_IdIsIncorrectButIdentifierIsCorrect(
            DistributedLockIdentifier identifier,
            DistributedLockId anotherId,
            DistributedLockTimeToLive timeToLive)
        {
            // make sure the timeout will last no longer than until we try to release it
            const int maxMillisecondsTimeout = 100;
            timeToLive = new DistributedLockTimeToLive(TimeSpan.FromMilliseconds(timeToLive.Value.TotalMilliseconds % maxMillisecondsTimeout));
            var (success, _) = await DistributedLockRepository.TryAcquireAsync(identifier, timeToLive, CancellationToken.None);
            Assert.True(success);

            await Task.Delay(maxMillisecondsTimeout);
            var result = await DistributedLockRepository.TryReleaseAsync(
                identifier,
                anotherId,
                CancellationToken.None);
            
            Assert.False(result);
        }

        [Theory]
        [AutoMoqData]
        public async Task ExtendAcquiredLock(
            DistributedLockIdentifier identifier,
            DistributedLockTimeToLive timeToLive)
        {
            // make sure the timeout will last at least until the release is called
            timeToLive = new DistributedLockTimeToLive(timeToLive.Value + TimeSpan.FromSeconds(5));
            var (success, acquiredLockId) = await DistributedLockRepository.TryAcquireAsync(identifier, timeToLive, CancellationToken.None);
            Assert.True(success);

            var result = await DistributedLockRepository.TryExtendAsync(
                identifier,
                acquiredLockId,
                timeToLive,
                CancellationToken.None);
            
            Assert.True(result);
        }

        [Theory]
        [AutoMoqData]
        public async Task FailToExtendAcquiredLock_When_TryingToExtendLock_And_TimeoutHasExpired(
            DistributedLockIdentifier identifier,
            DistributedLockTimeToLive timeToLive)
        {
            // make sure the timeout will last no longer than until we try to release it
            const int maxMillisecondsTimeout = 100;
            timeToLive = new DistributedLockTimeToLive(TimeSpan.FromMilliseconds(timeToLive.Value.TotalMilliseconds % maxMillisecondsTimeout));
            var (success, acquiredLockId) = await DistributedLockRepository.TryAcquireAsync(identifier, timeToLive, CancellationToken.None);
            Assert.True(success);

            await Task.Delay(maxMillisecondsTimeout);
            var result = await DistributedLockRepository.TryExtendAsync(
                identifier,
                acquiredLockId,
                timeToLive,
                CancellationToken.None);
            
            Assert.False(result);
        }

        [Theory]
        [AutoMoqData]
        public async Task FailToExtendAcquiredLock_When_TryingToExtendLock_And_IdIsCorrectButIdentifierIsNot(
            DistributedLockIdentifier identifier,
            DistributedLockIdentifier anotherIdentifier,
            DistributedLockTimeToLive timeToLive)
        {
            // make sure the timeout will last no longer than until we try to release it
            const int maxMillisecondsTimeout = 100;
            timeToLive = new DistributedLockTimeToLive(TimeSpan.FromMilliseconds(timeToLive.Value.TotalMilliseconds % maxMillisecondsTimeout));
            var (success, acquiredLockId) = await DistributedLockRepository.TryAcquireAsync(identifier, timeToLive, CancellationToken.None);
            Assert.True(success);

            await Task.Delay(maxMillisecondsTimeout);
            var result = await DistributedLockRepository.TryExtendAsync(
                anotherIdentifier,
                acquiredLockId,
                timeToLive,
                CancellationToken.None);
            
            Assert.False(result);
        }

        [Theory]
        [AutoMoqData]
        public async Task FailToExtendAcquiredLock_When_TryingToExtendLock_And_IdIsIncorrectButIdentifierIsCorrect(
            DistributedLockIdentifier identifier,
            DistributedLockId anotherId,
            DistributedLockTimeToLive timeToLive)
        {
            // make sure the timeout will last no longer than until we try to release it
            const int maxMillisecondsTimeout = 100;
            timeToLive = new DistributedLockTimeToLive(TimeSpan.FromMilliseconds(timeToLive.Value.TotalMilliseconds % maxMillisecondsTimeout));
            var (success, _) = await DistributedLockRepository.TryAcquireAsync(identifier, timeToLive, CancellationToken.None);
            Assert.True(success);

            await Task.Delay(maxMillisecondsTimeout);
            var result = await DistributedLockRepository.TryExtendAsync(
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
            var result = await DistributedLockRepository.TryExtendAsync(
                identifier,
                id,
                timeToLive,
                CancellationToken.None);
            
            Assert.False(result);
        }
    }
}