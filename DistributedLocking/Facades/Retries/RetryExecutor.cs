using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.Abstractions.Facades.Retries;
using DistributedLocking.Abstractions.Facades.Retries.Exceptions;
using DistributedLocking.Extensions;

namespace DistributedLocking.Facades.Retries
{
    public class RetryExecutor : IRetryExecutor
    {
        public async Task<T> ExecuteWithRetriesAsync<T>(
            Func<Task<(bool Success, T Result)>> func,
            IRetryPolicy retryPolicy, 
            CancellationToken cancellationToken)
        {
            func.EnsureIsNotNull(nameof(func));
            var stopwatch = Stopwatch.StartNew();
            uint retryNumber = 0;
            RetryExecutionMetadata metadata;

            do
            {
                var (success, result) = await func().ConfigureAwait(false);
                if (success)
                {
                    return result;
                }

                retryNumber++;
                metadata = new RetryExecutionMetadata(stopwatch, retryNumber);

                var delayBeforeNextRetry = retryPolicy.GetDelayBeforeNextRetry(metadata);
                if (retryPolicy.CanRetry(metadata) && delayBeforeNextRetry > TimeSpan.Zero)
                {
                    await Task.Delay(delayBeforeNextRetry, cancellationToken).ConfigureAwait(false);
                }
            } while (retryPolicy.CanRetry(metadata));

            throw new RetryExecutionFailedException();
        }
    }
}