using System;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedLocking.Abstractions.Facades.Retries
{
    public interface IRetryExecutor
    {
        Task<T> ExecuteWithRetriesAsync<T>(
            Func<Task<(bool Success, T Result)>> func,
            IRetryPolicy retryPolicy,
            CancellationToken cancellationToken);
    }
}