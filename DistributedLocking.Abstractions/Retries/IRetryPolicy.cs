using System;

namespace DistributedLocking.Abstractions.Retries
{
    public interface IRetryPolicy
    {
        TimeSpan DelayBetweenRetries { get; }
        
        bool CanRetry();
    }
}