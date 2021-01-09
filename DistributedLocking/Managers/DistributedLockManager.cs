using System;
using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.Abstractions;
using DistributedLocking.Abstractions.Exceptions;
using DistributedLocking.Abstractions.Managers;
using DistributedLocking.Abstractions.Records;
using DistributedLocking.Abstractions.Repositories;
using DistributedLocking.Abstractions.Retries;
using DistributedLocking.Extensions;

namespace DistributedLocking.Managers
{
    public class DistributedLockManager : IDistributedLockManager
    {
        private readonly IDistributedLockRepository _repository;
        private readonly IRetryExecutor _retryExecutor;

        public DistributedLockManager(
            IDistributedLockRepository repository,
            IRetryExecutor retryExecutor)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _retryExecutor = retryExecutor ?? throw new ArgumentNullException(nameof(retryExecutor));
        }
        
        public async Task<IDistributedLock> AcquireAsync(
            DistributedLockIdentifier lockIdentifier,
            DistributedLockTimeToLive lockTimeToLive,
            IRetryPolicyProvider retryPolicyProvider,
            CancellationToken cancellationToken)
        {
            lockIdentifier.EnsureIsNotNull(nameof(lockIdentifier));
            lockTimeToLive.EnsureIsNotNull(nameof(lockTimeToLive));
            retryPolicyProvider.EnsureIsNotNull(nameof(retryPolicyProvider));
            
            try
            {
                return await _retryExecutor.ExecuteWithRetriesAsync(
                        () => TryAcquireLockAsync(
                            lockIdentifier,
                            lockTimeToLive,
                            cancellationToken),
                        retryPolicyProvider,
                        cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                throw new CouldNotAcquireLockException(lockIdentifier, lockTimeToLive, exception);
            }
        }

        public async Task ExtendAsync(
            IDistributedLock distributedLock, 
            DistributedLockTimeToLive lockTimeToLive,
            IRetryPolicyProvider retryPolicyProvider,
            CancellationToken cancellationToken)
        {
            try
            {
                await _retryExecutor.ExecuteWithRetriesAsync(
                        () => _repository.TryExtendAsync(
                            distributedLock.LockIdentifier,
                            distributedLock.LockId,
                            lockTimeToLive,
                            cancellationToken),
                        retryPolicyProvider,
                        cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                throw new CouldNotExtendLockException(
                    distributedLock.LockIdentifier,
                    distributedLock.LockId,
                    exception);
            }
        }

        public async Task ReleaseAsync(
            IDistributedLock distributedLock,
            IRetryPolicyProvider retryPolicyProvider,
            CancellationToken cancellationToken)
        {
            try
            {
                await _retryExecutor.ExecuteWithRetriesAsync(
                        () => _repository.TryReleaseAsync(
                            distributedLock.LockIdentifier,
                            distributedLock.LockId,
                            cancellationToken),
                        retryPolicyProvider,
                        cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                throw new CouldNotReleaseLockException(
                    distributedLock.LockIdentifier,
                    distributedLock.LockId,
                    exception);
            }
        }

        private async Task<(bool Success, IDistributedLock Result)> TryAcquireLockAsync(
            DistributedLockIdentifier lockIdentifier,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken)
        {
            var (success, acquiredLockId) = await _repository.TryAcquireAsync(
                    lockIdentifier,
                    timeToLive,
                    cancellationToken)
                .ConfigureAwait(false);
                
            if (!success)
            {
                return (false, null);
            }

            return (
                true,
                new DistributedLock(
                    acquiredLockId,
                    lockIdentifier,
                    _repository));
        }
    }
}