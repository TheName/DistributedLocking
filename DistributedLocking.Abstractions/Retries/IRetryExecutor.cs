using System;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedLocking.Abstractions.Retries
{
    public interface IRetryExecutor
    {
        Task ExecuteWithRetriesAsync(
            Func<Task<bool>> func,
            IRetryPolicyProvider policyProvider,
            CancellationToken cancellationToken);
        
        Task<T> ExecuteWithRetriesAsync<T>(
            Func<Task<(bool Success, T Result)>> func,
            IRetryPolicyProvider policyProvider,
            CancellationToken cancellationToken);
    }
}