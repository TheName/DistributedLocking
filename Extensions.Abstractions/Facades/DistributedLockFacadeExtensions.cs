using System;
using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.Abstractions;
using DistributedLocking.Abstractions.Facades;
using DistributedLocking.Abstractions.Facades.Retries;

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
        
        public static async Task<IDistributedLock> AcquireAsync(
            this IDistributedLockFacade facade,
            DistributedLockIdentifier identifier,
            DistributedLockTimeToLive timeToLive,
            TimeSpan timeout,
            TimeSpan delayBetweenRetries,
            CancellationToken cancellationToken)
        {
            if (facade == null)
            {
                throw new ArgumentNullException(nameof(facade));
            }

            var policy = new TimeoutRetryPolicy(timeout, delayBetweenRetries);
            return await facade.AcquireAsync(
                    identifier,
                    timeToLive,
                    policy,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        
        public static async Task<IDistributedLock> AcquireAsync(
            this IDistributedLockFacade facade,
            DistributedLockIdentifier identifier,
            DistributedLockTimeToLive timeToLive,
            TimeSpan timeout,
            Func<RetryExecutionMetadata, TimeSpan> delayFunc,
            CancellationToken cancellationToken)
        {
            if (facade == null)
            {
                throw new ArgumentNullException(nameof(facade));
            }

            var policy = new TimeoutRetryPolicy(timeout, delayFunc);
            return await facade.AcquireAsync(
                    identifier,
                    timeToLive,
                    policy,
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

        public static async Task ExtendAsync(
            this IDistributedLockFacade facade,
            IDistributedLock distributedLock,
            DistributedLockTimeToLive timeToLive,
            TimeSpan timeout,
            TimeSpan delayBetweenRetries,
            CancellationToken cancellationToken)
        {
            if (facade == null)
            {
                throw new ArgumentNullException(nameof(facade));
            }

            var policy = new TimeoutRetryPolicy(timeout, delayBetweenRetries);
            await facade.ExtendAsync(
                    distributedLock,
                    timeToLive,
                    policy,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        public static async Task ExtendAsync(
            this IDistributedLockFacade facade,
            IDistributedLock distributedLock,
            DistributedLockTimeToLive timeToLive,
            TimeSpan timeout,
            Func<RetryExecutionMetadata, TimeSpan> delayFunc,
            CancellationToken cancellationToken)
        {
            if (facade == null)
            {
                throw new ArgumentNullException(nameof(facade));
            }

            var policy = new TimeoutRetryPolicy(timeout, delayFunc);
            await facade.ExtendAsync(
                    distributedLock,
                    timeToLive,
                    policy,
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

        public static async Task ReleaseAsync(
            this IDistributedLockFacade facade,
            IDistributedLock distributedLock,
            TimeSpan timeout,
            TimeSpan delayBetweenRetries,
            CancellationToken cancellationToken)
        {
            if (facade == null)
            {
                throw new ArgumentNullException(nameof(facade));
            }

            var policy = new TimeoutRetryPolicy(timeout, delayBetweenRetries);
            await facade.ReleaseAsync(
                    distributedLock,
                    policy,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        public static async Task ReleaseAsync(
            this IDistributedLockFacade facade,
            IDistributedLock distributedLock,
            TimeSpan timeout,
            Func<RetryExecutionMetadata, TimeSpan> delayFunc,
            CancellationToken cancellationToken)
        {
            if (facade == null)
            {
                throw new ArgumentNullException(nameof(facade));
            }

            var policy = new TimeoutRetryPolicy(timeout, delayFunc);
            await facade.ReleaseAsync(
                    distributedLock,
                    policy,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        private class NoRetriesPolicy : IRetryPolicy
        {
            public static readonly NoRetriesPolicy Instance = new(); 
        
            public bool CanRetry(RetryExecutionMetadata metadata) => false;

            public TimeSpan GetDelayBeforeNextRetry(RetryExecutionMetadata metadata) => TimeSpan.Zero;
        }
        
        private class TimeoutRetryPolicy : IRetryPolicy
        {
            private readonly TimeSpan _timeout;
            private readonly Func<RetryExecutionMetadata, TimeSpan> _delayFunc;

            public TimeoutRetryPolicy(TimeSpan timeout, TimeSpan delay) : this(timeout, _ => delay)
            {
            }

            public TimeoutRetryPolicy(TimeSpan timeout, Func<RetryExecutionMetadata, TimeSpan> delayFunc)
            {
                if (timeout < TimeSpan.Zero)
                {
                    throw new ArgumentException("Retry timeout cannot be lower than zero.");
                }
                _timeout = timeout;
                _delayFunc = delayFunc ?? throw new ArgumentNullException(nameof(delayFunc));
            }

            public bool CanRetry(RetryExecutionMetadata metadata) => metadata.Elapsed < _timeout;

            public TimeSpan GetDelayBeforeNextRetry(RetryExecutionMetadata metadata) => _delayFunc(metadata);
        }
    }
}