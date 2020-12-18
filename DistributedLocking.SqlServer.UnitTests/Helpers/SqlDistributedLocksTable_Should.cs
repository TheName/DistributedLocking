using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using TestHelpers.Attributes;
using TheName.DistributedLocking.SqlServer.Abstractions.Helpers;
using TheName.DistributedLocking.SqlServer.Helpers;
using Xunit;

namespace DistributedLocking.SqlServer.UnitTests.Helpers
{
    public class SqlDistributedLocksTable_Should
    {
        private Mock<ISqlClient> SqlClientMock { get; } = new();

        private SqlDistributedLocksTable SqlDistributedLocksTable => new(SqlClientMock.Object);
        
        [Fact]
        public void Throw_When_Creating_And_SqlClientIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlDistributedLocksTable(null));
        }
        
        [Fact]
        public void NotThrow_When_Creating_And_SqlClientIsNotNull()
        {
            _ = new SqlDistributedLocksTable(SqlClientMock.Object);
        }

        [Theory]
        [AutoMoqData]
        public async Task PassExpectedCommandText_When_TryingToInsertAsync(
            string schemaName,
            string tableName,
            Guid lockIdentifier,
            Guid lockId,
            TimeSpan expirationTimeSpan)
        {
            var expectedCommandText = GetExpectedInsertIfNotExistsCommandText(schemaName, tableName);
            
            await SqlDistributedLocksTable.TryInsertAsync(
                schemaName,
                tableName,
                lockIdentifier,
                lockId,
                expirationTimeSpan,
                CancellationToken.None);

            SqlClientMock
                .Verify(
                    client => client.ExecuteNonQueryAsync(
                        expectedCommandText,
                        It.IsAny<SqlParameter[]>(),
                        It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        [Theory]
        [AutoMoqData]
        public async Task PassExpectedSqlParameters_When_TryingToInsertAsync(
            string schemaName,
            string tableName,
            Guid lockIdentifier,
            Guid lockId,
            TimeSpan expirationTimeSpan)
        {
            await SqlDistributedLocksTable.TryInsertAsync(
                schemaName,
                tableName,
                lockIdentifier,
                lockId,
                expirationTimeSpan,
                CancellationToken.None);

            var assertSqlParameters = new Func<SqlParameter[], bool>(parameters =>
            {
                Assert.Equal(3, parameters.Length);
                
                var lockIdentifierParameter = parameters.Single(parameter => parameter.ParameterName == "@LockIdentifier");
                Assert.Equal(SqlDbType.Char, lockIdentifierParameter.SqlDbType);
                Assert.Equal(lockIdentifier.ToString().Length, lockIdentifierParameter.Size);
                Assert.Equal(lockIdentifier.ToString(), lockIdentifierParameter.Value);
                
                var lockIdParameter = parameters.Single(parameter => parameter.ParameterName == "@LockId");
                Assert.Equal(SqlDbType.Char, lockIdParameter.SqlDbType);
                Assert.Equal(lockId.ToString().Length, lockIdParameter.Size);
                Assert.Equal(lockId.ToString(), lockIdParameter.Value);
                
                var expiryDateTimeSpanInMillisecondsParameter = parameters.Single(parameter => parameter.ParameterName == "@ExpiryDateTimeSpanInMilliseconds");
                Assert.Equal(SqlDbType.BigInt, expiryDateTimeSpanInMillisecondsParameter.SqlDbType);
                Assert.Equal(expirationTimeSpan.TotalMilliseconds, expiryDateTimeSpanInMillisecondsParameter.Value);

                return true;
            });

            SqlClientMock
                .Verify(
                    client => client.ExecuteNonQueryAsync(
                        It.IsAny<string>(),
                        It.Is<SqlParameter[]>(parameters => assertSqlParameters(parameters)),
                        It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        [Theory]
        [AutoMoqWithInlineData(false, 0)]
        [AutoMoqWithInlineData(true, 1)]
        public async Task ReturnExpectedResult_When_TryingToInsertAsync_And_SqlClientReturnsGivenNumberOfAffectedRows(
            bool expectedResult,
            int numberOfAffectedRows,
            string schemaName,
            string tableName,
            Guid lockIdentifier,
            Guid lockId,
            TimeSpan expirationTimeSpan)
        {
            SqlClientMock
                .Setup(client => client.ExecuteNonQueryAsync(
                    It.IsAny<string>(),
                    It.IsAny<SqlParameter[]>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(numberOfAffectedRows);
            
            var result = await SqlDistributedLocksTable.TryInsertAsync(
                schemaName,
                tableName,
                lockIdentifier,
                lockId,
                expirationTimeSpan,
                CancellationToken.None);
            
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [AutoMoqWithInlineData(-1)]
        [AutoMoqWithInlineData(2)]
        public async Task Throw_When_TryingToInsertAsync_And_SqlClientReturnsUnexpectedNumberOfAffectedRows(
            int numberOfAffectedRows,
            string schemaName,
            string tableName,
            Guid lockIdentifier,
            Guid lockId,
            TimeSpan expirationTimeSpan)
        {
            SqlClientMock
                .Setup(client => client.ExecuteNonQueryAsync(
                    It.IsAny<string>(),
                    It.IsAny<SqlParameter[]>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(numberOfAffectedRows);

            await Assert.ThrowsAsync<InvalidOperationException>(() => SqlDistributedLocksTable.TryInsertAsync(
                schemaName,
                tableName,
                lockIdentifier,
                lockId,
                expirationTimeSpan,
                CancellationToken.None));
        }

        [Theory]
        [AutoMoqData]
        public async Task PassExpectedCommandText_When_TryingToDeleteAsync(
            string schemaName,
            string tableName,
            Guid lockId)
        {
            var expectedCommandText = GetExpectedDeleteCommandText(schemaName, tableName);
            
            await SqlDistributedLocksTable.TryDeleteAsync(
                schemaName,
                tableName,
                lockId,
                CancellationToken.None);

            SqlClientMock
                .Verify(
                    client => client.ExecuteNonQueryAsync(
                        expectedCommandText,
                        It.IsAny<SqlParameter[]>(),
                        It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        [Theory]
        [AutoMoqData]
        public async Task PassExpectedSqlParameters_When_TryingToDeleteAsync(
            string schemaName,
            string tableName,
            Guid lockId)
        {
            await SqlDistributedLocksTable.TryDeleteAsync(
                schemaName,
                tableName,
                lockId,
                CancellationToken.None);

            var assertSqlParameters = new Func<SqlParameter[], bool>(parameters =>
            {
                var singleParameter = Assert.Single(parameters);
                Assert.NotNull(singleParameter);
                
                Assert.Equal("@LockId", singleParameter.ParameterName);
                Assert.Equal(SqlDbType.Char, singleParameter.SqlDbType);
                Assert.Equal(lockId.ToString().Length, singleParameter.Size);
                Assert.Equal(lockId.ToString(), singleParameter.Value);

                return true;
            });

            SqlClientMock
                .Verify(
                    client => client.ExecuteNonQueryAsync(
                        It.IsAny<string>(),
                        It.Is<SqlParameter[]>(parameters => assertSqlParameters(parameters)),
                        It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        [Theory]
        [AutoMoqWithInlineData(false, 0)]
        [AutoMoqWithInlineData(true, 1)]
        public async Task ReturnExpectedResult_When_TryingToDeleteAsync_And_SqlClientReturnsGivenNumberOfAffectedRows(
            bool expectedResult,
            int numberOfAffectedRows,
            string schemaName,
            string tableName,
            Guid lockId)
        {
            SqlClientMock
                .Setup(client => client.ExecuteNonQueryAsync(
                    It.IsAny<string>(),
                    It.IsAny<SqlParameter[]>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(numberOfAffectedRows);
            
            var result = await SqlDistributedLocksTable.TryDeleteAsync(
                schemaName,
                tableName,
                lockId,
                CancellationToken.None);
            
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [AutoMoqWithInlineData(-1)]
        [AutoMoqWithInlineData(2)]
        public async Task Throw_When_TryingToDeleteAsync_And_SqlClientReturnsUnexpectedNumberOfAffectedRows(
            int numberOfAffectedRows,
            string schemaName,
            string tableName,
            Guid lockId)
        {
            SqlClientMock
                .Setup(client => client.ExecuteNonQueryAsync(
                    It.IsAny<string>(),
                    It.IsAny<SqlParameter[]>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(numberOfAffectedRows);

            await Assert.ThrowsAsync<InvalidOperationException>(() => SqlDistributedLocksTable.TryDeleteAsync(
                schemaName,
                tableName,
                lockId,
                CancellationToken.None));
        }

        private static string GetExpectedInsertIfNotExistsCommandText(string schemaName, string tableName) =>
            "BEGIN TRANSACTION; " +
            "SET NOCOUNT ON; " +
            $"DELETE FROM [{schemaName}].[{tableName}] " +
            "  WHERE LockIdentifier = @LockIdentifier AND ExpiryDateTimestamp < SYSUTCDATETIME(); " +
            "SET NOCOUNT OFF; " +
            "COMMIT TRANSACTION; " +
            "BEGIN TRANSACTION; " +
            $"INSERT INTO [{schemaName}].[{tableName}] " +
            "SELECT " +
            "   @LockIdentifier," +
            "   @LockId," +
            "   DATEADD(millisecond,@ExpiryDateTimeSpanInMilliseconds,SYSUTCDATETIME()) " +
            "WHERE" +
            "   NOT EXISTS " +
            "   (SELECT *" +
            $"   FROM [{schemaName}].[{tableName}] WITH (UPDLOCK, HOLDLOCK)" +
            "   WHERE  LockIdentifier = @LockIdentifier" +
            "    AND    ExpiryDateTimestamp > SYSUTCDATETIME()); " +
            "COMMIT TRANSACTION; ";

        private static string GetExpectedDeleteCommandText(string schemaName, string tableName) =>
            $"DELETE FROM [{schemaName}].[{tableName}] " +
            "WHERE LockId = @LockId " +
            "AND    ExpiryDateTimestamp < SYSUTCDATETIME();";
    }
}