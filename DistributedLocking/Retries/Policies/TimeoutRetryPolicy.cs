using System;
using DistributedLocking.Abstractions.Retries;

namespace DistributedLocking.Retries.Policies
{
    public class TimeoutRetryPolicy : IRetryPolicy
    {
        private readonly TimeSpan _timeout;
        private readonly Func<RetryExecutionMetadata, TimeSpan> _delayFunc;

        public TimeoutRetryPolicy(TimeSpan timeout, TimeSpan delay) : this(timeout, _ => delay)
        {
        }

        public TimeoutRetryPolicy(TimeSpan timeout, Func<RetryExecutionMetadata, TimeSpan> delayFunc)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw new ArgumentException("Retry timeout cannot be lower than zero.");
            }
            _timeout = timeout;
            _delayFunc = delayFunc ?? throw new ArgumentNullException(nameof(delayFunc));
        }

        public bool CanRetry(RetryExecutionMetadata metadata) => metadata.Elapsed < _timeout;

        public TimeSpan GetDelayBeforeNextRetry(RetryExecutionMetadata metadata) => _delayFunc(metadata);
    }
}