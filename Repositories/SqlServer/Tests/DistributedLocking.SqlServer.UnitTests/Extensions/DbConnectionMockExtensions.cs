using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;

namespace DistributedLocking.SqlServer.UnitTests.Extensions
{
    public static class DbConnectionMockExtensions
    {
        public static Mock<DbConnection> SetupDbCommand(this Mock<DbConnection> dbConnectionMock, Mock<DbCommand> dbCommandMock)
        {
            dbConnectionMock
                .Protected()
                .Setup<DbCommand>("CreateDbCommand")
                .Returns(() =>
                {
                    dbCommandMock
                        .Protected()
                        .SetupGet<DbConnection>("DbConnection")
                        .Returns(dbConnectionMock.Object);
                    
                    return dbCommandMock.Object;
                });

            return dbConnectionMock;
        }
        
        public static Mock<DbConnection> SetupOpenAsync(this Mock<DbConnection> dbConnectionMock, ConnectionState connectionState)
        {
            dbConnectionMock
                .Setup(connection => connection.OpenAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Callback<CancellationToken>(_ => dbConnectionMock
                    .SetupGet(connection => connection.State)
                    .Returns(connectionState));

            return dbConnectionMock;
        }
    }
}