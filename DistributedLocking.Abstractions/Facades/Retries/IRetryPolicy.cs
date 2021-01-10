using System;

namespace DistributedLocking.Abstractions.Facades.Retries
{
    public interface IRetryPolicy
    {
        bool CanRetry(RetryExecutionMetadata metadata);

        TimeSpan GetDelayBeforeNextRetry(RetryExecutionMetadata metadata);
    }
}