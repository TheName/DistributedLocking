using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.SqlServer.Abstractions.Helpers;
using DistributedLocking.SqlServer.Helpers;
using DistributedLocking.SqlServer.UnitTests.Extensions;
using Moq;
using TestHelpers.Attributes;
using Xunit;

namespace DistributedLocking.SqlServer.UnitTests.Helpers
{
    public class SqlClient_Should
    {
        private Mock<ISqlConnectionFactory> SqlConnectionFactoryMock { get; } = new();

        private SqlClient SqlClient => new(SqlConnectionFactoryMock.Object);
        
        [Fact]
        public void Throw_When_Creating_And_SqlConnectionFactoryIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlClient(null));
        }
        
        [Fact]
        public void NotThrow_When_Creating_And_SqlConnectionFactoryIsNotNull()
        {
            _ = new SqlClient(SqlConnectionFactoryMock.Object);
        }

        [Theory]
        [AutoMoqData]
        public async Task ExecuteScalarCommand_And_ReturnDefault_When_CallingExecuteScalarAsync_And_ResultIsNull(
            string sqlCommandText)
        {
            var dbCommandMock = new Mock<DbCommand>().SetupExecuteScalarAsync(sqlCommandText, null);
            var dbConnectionMock = CreateAndSetupDbConnectionMock(dbCommandMock);

            SqlConnectionFactoryMock
                .Setup(factory => factory.Create())
                .Returns(dbConnectionMock.Object);

            var result = await SqlClient.ExecuteScalarAsync<Guid>(sqlCommandText, CancellationToken.None);
            
            Assert.Equal(Guid.Empty, result);
            dbConnectionMock.Verify(connection => connection.DisposeAsync(), Times.Once);
        }

        [Theory]
        [AutoMoqData]
        public async Task ExecuteScalarCommand_And_ReturnExecutionResult_When_CallingExecuteScalarAsync_And_ResultIsNotNull(
            string sqlCommandText,
            Guid scalarExecutionResult)
        {
            var dbCommandMock = new Mock<DbCommand>().SetupExecuteScalarAsync(sqlCommandText, scalarExecutionResult);
            var dbConnectionMock = CreateAndSetupDbConnectionMock(dbCommandMock);

            SqlConnectionFactoryMock
                .Setup(factory => factory.Create())
                .Returns(dbConnectionMock.Object);

            var result = await SqlClient.ExecuteScalarAsync<Guid>(sqlCommandText, CancellationToken.None);
            
            Assert.Equal(scalarExecutionResult, result);
            dbConnectionMock.Verify(connection => connection.DisposeAsync(), Times.Once);
        }

        [Theory]
        [AutoMoqData]
        public async Task ExecuteNonQueryCommand_And_ReturnExecutionResult_When_CallingExecuteNonQueryAsync(
            string sqlCommandText,
            SqlParameter[] sqlParameters,
            int executionResult)
        {
            var dbCommandMock = new Mock<DbCommand>().SetupExecuteNonQueryAsync(sqlCommandText, sqlParameters, executionResult);
            var dbConnectionMock = CreateAndSetupDbConnectionMock(dbCommandMock);

            SqlConnectionFactoryMock
                .Setup(factory => factory.Create())
                .Returns(dbConnectionMock.Object);

            var result = await SqlClient.ExecuteNonQueryAsync(sqlCommandText, sqlParameters, CancellationToken.None);
            
            Assert.Equal(executionResult, result);
            dbConnectionMock.Verify(connection => connection.DisposeAsync(), Times.Once);
        }

        private static Mock<DbConnection> CreateAndSetupDbConnectionMock(Mock<DbCommand> dbCommand) =>
            new Mock<DbConnection>()
                .SetupDbCommand(dbCommand)
                .SetupOpenAsync(ConnectionState.Open);
    }
}