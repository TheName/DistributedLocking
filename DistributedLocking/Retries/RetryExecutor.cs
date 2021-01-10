using System;
using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.Abstractions.Exceptions;
using DistributedLocking.Abstractions.Retries;

namespace DistributedLocking.Retries
{
    public class RetryExecutor : IRetryExecutor
    {
        public async Task<T> ExecuteWithRetriesAsync<T>(
            Func<Task<(bool Success, T Result)>> func,
            IRetryPolicyProvider policyProvider, 
            CancellationToken cancellationToken)
        {
            var policy = policyProvider.CreateNew();

            do
            {
                var (success, result) = await func().ConfigureAwait(false);
                if (success)
                {
                    return result;
                }

                if (policy.CanRetry() && policy.DelayBetweenRetries > TimeSpan.Zero)
                {
                    await Task.Delay(policy.DelayBetweenRetries, cancellationToken).ConfigureAwait(false);
                }
            } while (policy.CanRetry());

            throw new RetryExecutionFailedException();
        }
    }
}