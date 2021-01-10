﻿using System;

namespace DistributedLocking.Abstractions.Retries
{
    public interface IRetryPolicy
    {
        bool CanRetry(RetryExecutionMetadata metadata);

        TimeSpan GetDelayBeforeNextRetry(RetryExecutionMetadata metadata);
    }
}