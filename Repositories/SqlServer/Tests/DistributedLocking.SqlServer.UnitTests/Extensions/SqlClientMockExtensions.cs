using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using DistributedLocking.Repositories.SqlServer.Abstractions.Helpers;
using Moq;

namespace DistributedLocking.SqlServer.UnitTests.Extensions
{
    internal static class SqlClientMockExtensions
    {
        public static Mock<ISqlClient> SetupNonQueryResult(this Mock<ISqlClient> sqlClientMock, int result)
        {
            sqlClientMock
                .Setup(client => client.ExecuteNonQueryAsync(
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<SqlParameter>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            return sqlClientMock;
        }
    }
}