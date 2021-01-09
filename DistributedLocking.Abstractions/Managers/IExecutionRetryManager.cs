using System;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedLocking.Abstractions.Managers
{
    public interface IExecutionRetryManager
    {
        Task<T> RetryAsync<T>(Func<Task<(bool, T)>> action, CancellationToken cancellationToken);
    }
}