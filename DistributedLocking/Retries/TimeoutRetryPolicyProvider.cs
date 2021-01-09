using System;
using DistributedLocking.Abstractions.Retries;

namespace DistributedLocking.Retries
{
    public class TimeoutRetryPolicyProvider : IRetryPolicyProvider
    {
        private readonly TimeoutRetryPolicyConfiguration _configuration;

        public TimeoutRetryPolicyProvider(TimeoutRetryPolicyConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public IRetryPolicy CreateNew() => new TimeoutRetryPolicy(_configuration);
    }
}