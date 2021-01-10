using System;
using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.Abstractions.Facades.Retries;
using Moq;

namespace DistributedLocking.UnitTests.Extensions
{
    internal static class RetryExecutorMockExtensions
    {
        public static void SetupException<T>(this Mock<IRetryExecutor> retryExecutorMock, Exception exception)
        {
            retryExecutorMock
                .Setup(executor => executor.ExecuteWithRetriesAsync(
                    It.IsAny<Func<Task<(bool, T)>>>(),
                    It.IsAny<IRetryPolicy>(),
                    It.IsAny<CancellationToken>()))
                .Throws(exception);
        }
        
        public static void SetupException(this Mock<IRetryExecutor> retryExecutorMock, Exception exception)
        {
            retryExecutorMock.SetupException<object>(exception);
        }
        
        public static void Setup<T>(this Mock<IRetryExecutor> retryExecutorMock)
        {
            retryExecutorMock
                .Setup(executor => executor.ExecuteWithRetriesAsync(
                    It.IsAny<Func<Task<(bool, T)>>>(),
                    It.IsAny<IRetryPolicy>(),
                    It.IsAny<CancellationToken>()))
                .Returns<Func<Task<(bool, T)>>, IRetryPolicy, CancellationToken>(
                    async (func, _, _) =>
                    {
                        var (_, result) = await func();
                        return result;
                    });
        }
        
        public static void Setup(this Mock<IRetryExecutor> retryExecutorMock)
        {
            retryExecutorMock.Setup<object>();
        }
    }
}