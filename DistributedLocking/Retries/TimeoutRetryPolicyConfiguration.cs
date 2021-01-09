using System;

namespace DistributedLocking.Retries
{
    public record TimeoutRetryPolicyConfiguration
    {
        private readonly TimeSpan _timeout;
        private readonly TimeSpan _retryDelay;

        public TimeSpan Timeout
        {
            get => _timeout;
            init
            {
                if (value < TimeSpan.Zero)
                {
                    throw new ArgumentException("Timeout cannot be lower than zero!", nameof(Timeout));
                }

                _timeout = value;
            }
        }

        public TimeSpan RetryDelay
        {
            get => _retryDelay;
            init
            {
                if (value < TimeSpan.Zero)
                {
                    throw new ArgumentException("RetryDelay cannot be lower than zero!", nameof(RetryDelay));
                }

                _retryDelay = value;
            }
        }

        public TimeoutRetryPolicyConfiguration(
            TimeSpan timeout,
            TimeSpan retryDelay)
        {
            Timeout = timeout;
            RetryDelay = retryDelay;
        }
    }
}