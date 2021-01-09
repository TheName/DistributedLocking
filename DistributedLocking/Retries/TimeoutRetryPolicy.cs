using System;
using System.Diagnostics;
using DistributedLocking.Abstractions.Retries;

namespace DistributedLocking.Retries
{
    public class TimeoutRetryPolicy : IRetryPolicy
    {
        private readonly TimeoutRetryPolicyConfiguration _configuration;
        private readonly Stopwatch _stopwatch;

        public TimeoutRetryPolicy(TimeoutRetryPolicyConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _stopwatch = Stopwatch.StartNew();
        }

        public TimeSpan DelayBetweenRetries => _configuration.RetryDelay;

        public bool CanRetry() => _stopwatch.Elapsed < _configuration.Timeout;
    }
}