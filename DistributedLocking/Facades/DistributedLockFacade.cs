using System;
using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.Abstractions;
using DistributedLocking.Abstractions.Exceptions;
using DistributedLocking.Abstractions.Facades;
using DistributedLocking.Abstractions.Records;
using DistributedLocking.Abstractions.Repositories;
using DistributedLocking.Abstractions.Retries;
using DistributedLocking.Extensions;
using DistributedLocking.Extensions.Abstractions.Repositories;
using DistributedLocking.Extensions.Abstractions.Retries;

namespace DistributedLocking.Facades
{
    public class DistributedLockFacade : IDistributedLockFacade
    {
        private readonly IDistributedLockRepository _repository;
        private readonly IRetryExecutor _retryExecutor;

        public DistributedLockFacade(
            IDistributedLockRepository repository,
            IRetryExecutor retryExecutor)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _retryExecutor = retryExecutor ?? throw new ArgumentNullException(nameof(retryExecutor));
        }
        
        public async Task<IDistributedLock> AcquireAsync(
            DistributedLockIdentifier identifier,
            DistributedLockTimeToLive timeToLive,
            IRetryPolicyProvider retryPolicyProvider,
            CancellationToken cancellationToken)
        {
            identifier.EnsureIsNotNull(nameof(identifier));
            timeToLive.EnsureIsNotNull(nameof(timeToLive));
            retryPolicyProvider.EnsureIsNotNull(nameof(retryPolicyProvider));
            
            try
            {
                return await _retryExecutor.ExecuteWithRetriesAsync(
                        () => _repository.TryAcquireLockAsync(
                            identifier,
                            timeToLive,
                            cancellationToken),
                        retryPolicyProvider,
                        cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                throw new CouldNotAcquireLockException(identifier, timeToLive, exception);
            }
        }

        public async Task ExtendAsync(
            IDistributedLock distributedLock, 
            DistributedLockTimeToLive timeToLive,
            IRetryPolicyProvider retryPolicyProvider,
            CancellationToken cancellationToken)
        {
            try
            {
                await _retryExecutor.ExecuteWithRetriesAsync(
                        () => _repository.TryExtendAsync(
                            distributedLock,
                            timeToLive,
                            cancellationToken),
                        retryPolicyProvider,
                        cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                throw new CouldNotExtendLockException(
                    distributedLock.Identifier,
                    distributedLock.Id,
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
                    distributedLock.Identifier,
                    distributedLock.Id,
                    exception);
            }
        }
    }
}