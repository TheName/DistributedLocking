using System;
using System.Diagnostics;

namespace DistributedLocking.Abstractions.Facades.Retries
{
    public class RetryExecutionMetadata
    {
        private readonly Stopwatch _stopwatch;

        public TimeSpan Elapsed => _stopwatch.Elapsed;
        
        public uint RetryNumber { get; }

        public RetryExecutionMetadata(
            Stopwatch stopwatch,
            uint retryNumber)
        {
            _stopwatch = stopwatch ?? throw new ArgumentNullException(nameof(stopwatch));
            if (retryNumber == 0)
            {
                throw new ArgumentException("Retry number cannot be 0");
            }
            RetryNumber = retryNumber;
        }
    }
}