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
using DistributedLocking.Extensions.Abstractions.Repositories;

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
                        () => _repository.TryAcquireLockAsync(
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
                            distributedLock,
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
                            distributedLock,
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
    }
}