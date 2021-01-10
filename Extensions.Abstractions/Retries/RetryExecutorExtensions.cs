using System;
using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.Abstractions.Retries;

namespace DistributedLocking.Extensions.Abstractions.Retries
{
    public static class RetryExecutorExtensions
    {
        public static async Task ExecuteWithRetriesAsync(
            this IRetryExecutor retryExecutor,
            Func<Task<bool>> func,
            IRetryPolicyProvider policyProvider,
            CancellationToken cancellationToken)
        {
            if (retryExecutor == null)
            {
                throw new ArgumentNullException(nameof(retryExecutor));
            }

            await retryExecutor.ExecuteWithRetriesAsync<object>(
                    async () => (await func().ConfigureAwait(false), null),
                    policyProvider,
                    cancellationToken)
                .ConfigureAwait(false);
        }
    }
}