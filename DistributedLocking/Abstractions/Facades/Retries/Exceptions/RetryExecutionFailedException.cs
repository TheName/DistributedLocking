using System;

namespace DistributedLocking.Abstractions.Facades.Retries.Exceptions
{
    public class RetryExecutionFailedException : Exception
    {
        public RetryExecutionFailedException() : base(CreateExceptionMessage())
        {
        }

        private static string CreateExceptionMessage() =>
            "Executing action with retries did not result in a successful result.";
    }
}