using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using Moq;
using Moq.Protected;
using BindingFlags = System.Reflection.BindingFlags;

namespace DistributedLocking.SqlServer.UnitTests.Extensions
{
    public static class DbCommandMockExtensions
    {
        public static Mock<DbCommand> SetupExecuteScalarAsync(
            this Mock<DbCommand> dbCommandMock,
            string expectedSqlCommandText,
            object result)
        {
            dbCommandMock.SetupAllProperties();
            dbCommandMock
                .Setup(command => command.ExecuteScalarAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(() =>
                {
                    if (dbCommandMock.Object.CommandText != expectedSqlCommandText)
                    {
                        throw new Exception("Command text was not setup.");
                    }

                    if (dbCommandMock.Object.Connection?.State == ConnectionState.Open)
                    {
                        return result;
                    }

                    throw new Exception("State was not opened");
                });

            return dbCommandMock;
        }
        
        public static Mock<DbCommand> SetupExecuteNonQueryAsync(
            this Mock<DbCommand> dbCommandMock,
            string expectedSqlCommandText,
            SqlParameter[] sqlParameters,
            int result)
        {
            dbCommandMock.SetupAllProperties();
            dbCommandMock
                .Protected()
                .SetupGet<DbParameterCollection>("DbParameterCollection")
                .Returns((SqlParameterCollection) typeof(SqlParameterCollection)
                    .GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[0], null).Invoke(null));
            
            dbCommandMock
                .Setup(command => command.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(() =>
                {
                    if (dbCommandMock.Object.CommandText != expectedSqlCommandText)
                    {
                        throw new Exception("Command text was not setup.");
                    }

                    if (sqlParameters.Any(parameter => !dbCommandMock.Object.Parameters.Contains(parameter)))
                    {
                        throw new Exception("At least parameter was not setup properly.");
                    }

                    if (dbCommandMock.Object.Connection?.State == ConnectionState.Open)
                    {
                        return result;
                    }

                    throw new Exception("State was not opened");
                });

            return dbCommandMock;
        }
    }
}