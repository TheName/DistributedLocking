using System;
using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.Abstractions;
using DistributedLocking.Abstractions.Facades;
using DistributedLocking.Abstractions.Retries;

namespace DistributedLocking.Extensions.Abstractions.Facades
{
    public static class DistributedLockFacadeExtensions
    {
        public static async Task<IDistributedLock> AcquireAsync(
            this IDistributedLockFacade facade,
            DistributedLockIdentifier identifier,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken)
        {
            if (facade == null)
            {
                throw new ArgumentNullException(nameof(facade));
            }

            return await facade.AcquireAsync(
                    identifier,
                    timeToLive,
                    NoRetriesPolicy.Instance,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        public static async Task ExtendAsync(
            this IDistributedLockFacade facade,
            IDistributedLock distributedLock,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken)
        {
            if (facade == null)
            {
                throw new ArgumentNullException(nameof(facade));
            }

            await facade.ExtendAsync(
                    distributedLock,
                    timeToLive,
                    NoRetriesPolicy.Instance,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        public static async Task ReleaseAsync(
            this IDistributedLockFacade facade,
            IDistributedLock distributedLock,
            CancellationToken cancellationToken)
        {
            if (facade == null)
            {
                throw new ArgumentNullException(nameof(facade));
            }

            await facade.ReleaseAsync(
                    distributedLock,
                    NoRetriesPolicy.Instance,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        private class NoRetriesPolicy : IRetryPolicy
        {
            public static readonly NoRetriesPolicy Instance = new(); 
        
            public bool CanRetry(RetryExecutionMetadata metadata) => false;

            public TimeSpan GetDelayBeforeNextRetry(RetryExecutionMetadata metadata) => TimeSpan.Zero;
        }
    }
}