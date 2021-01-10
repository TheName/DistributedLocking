using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.SqlServer.Abstractions.Helpers;
using DistributedLocking.SqlServer.Helpers;
using Moq;
using TestHelpers.Attributes;
using Xunit;

namespace DistributedLocking.SqlServer.UnitTests.Helpers
{
    public class SqlDistributedLocksTable_Should
    {
        private const string ExpectedIdentifierParameterName = "@Identifier";
        private const string ExpectedIdParameterName = "@Id";
        private const string ExpectedExpiryDateTimeSpanInMillisecondsParameterName = "@ExpiryDateTimeSpanInMilliseconds";
        
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
            Guid identifier,
            Guid id,
            TimeSpan timeToLive)
        {
            var expectedCommandText = GetExpectedInsertIfNotExistsCommandText(schemaName, tableName);
            
            await SqlDistributedLocksTable.TryInsertAsync(
                schemaName,
                tableName,
                identifier,
                id,
                timeToLive,
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
            Guid identifier,
            Guid id,
            TimeSpan timeToLive)
        {
            await SqlDistributedLocksTable.TryInsertAsync(
                schemaName,
                tableName,
                identifier,
                id,
                timeToLive,
                CancellationToken.None);

            var assertSqlParameters = new Func<SqlParameter[], bool>(parameters =>
            {
                Assert.Equal(3, parameters.Length);
                
                var lockIdentifierParameter = parameters.Single(parameter => parameter.ParameterName == ExpectedIdentifierParameterName);
                Assert.Equal(SqlDbType.Char, lockIdentifierParameter.SqlDbType);
                Assert.Equal(identifier.ToString().Length, lockIdentifierParameter.Size);
                Assert.Equal(identifier.ToString(), lockIdentifierParameter.Value);
                
                var lockIdParameter = parameters.Single(parameter => parameter.ParameterName == ExpectedIdParameterName);
                Assert.Equal(SqlDbType.Char, lockIdParameter.SqlDbType);
                Assert.Equal(id.ToString().Length, lockIdParameter.Size);
                Assert.Equal(id.ToString(), lockIdParameter.Value);
                
                var expiryDateTimeSpanInMillisecondsParameter = parameters.Single(parameter => parameter.ParameterName == ExpectedExpiryDateTimeSpanInMillisecondsParameterName);
                Assert.Equal(SqlDbType.BigInt, expiryDateTimeSpanInMillisecondsParameter.SqlDbType);
                Assert.Equal(timeToLive.TotalMilliseconds, expiryDateTimeSpanInMillisecondsParameter.Value);

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
            Guid identifier,
            Guid id,
            TimeSpan timeToLive)
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
                identifier,
                id,
                timeToLive,
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
            Guid identifier,
            Guid id,
            TimeSpan timeToLive)
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
                identifier,
                id,
                timeToLive,
                CancellationToken.None));
        }

        [Theory]
        [AutoMoqData]
        public async Task PassExpectedCommandText_When_TryingToDeleteAsync(
            string schemaName,
            string tableName,
            Guid identifier,
            Guid id)
        {
            var expectedCommandText = GetExpectedDeleteCommandText(schemaName, tableName);
            
            await SqlDistributedLocksTable.TryDeleteAsync(
                schemaName,
                tableName,
                identifier,
                id,
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
            Guid identifier,
            Guid id)
        {
            await SqlDistributedLocksTable.TryDeleteAsync(
                schemaName,
                tableName,
                identifier,
                id,
                CancellationToken.None);

            var assertSqlParameters = new Func<SqlParameter[], bool>(parameters =>
            {
                Assert.Equal(2, parameters.Length);
                
                var lockIdentifierParameter = parameters.Single(parameter => parameter.ParameterName == ExpectedIdentifierParameterName);
                Assert.Equal(SqlDbType.Char, lockIdentifierParameter.SqlDbType);
                Assert.Equal(identifier.ToString().Length, lockIdentifierParameter.Size);
                Assert.Equal(identifier.ToString(), lockIdentifierParameter.Value);
                
                var lockIdParameter = parameters.Single(parameter => parameter.ParameterName == ExpectedIdParameterName);
                Assert.Equal(SqlDbType.Char, lockIdParameter.SqlDbType);
                Assert.Equal(id.ToString().Length, lockIdParameter.Size);
                Assert.Equal(id.ToString(), lockIdParameter.Value);

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
            Guid identifier,
            Guid id)
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
                identifier,
                id,
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
            Guid identifier,
            Guid id)
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
                identifier,
                id,
                CancellationToken.None));
        }

        [Theory]
        [AutoMoqData]
        public async Task PassExpectedCommandText_When_TryingToUpdateAsync(
            string schemaName,
            string tableName,
            Guid identifier,
            Guid id,
            TimeSpan timeToLive)
        {
            var expectedCommandText = GetExpectedUpdateCommandText(schemaName, tableName);
            
            await SqlDistributedLocksTable.TryUpdateAsync(
                schemaName,
                tableName,
                identifier,
                id,
                timeToLive,
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
        public async Task PassExpectedSqlParameters_When_TryingToUpdateAsync(
            string schemaName,
            string tableName,
            Guid identifier,
            Guid id,
            TimeSpan timeToLive)
        {
            await SqlDistributedLocksTable.TryUpdateAsync(
                schemaName,
                tableName,
                identifier,
                id,
                timeToLive,
                CancellationToken.None);

            var assertSqlParameters = new Func<SqlParameter[], bool>(parameters =>
            {
                Assert.Equal(3, parameters.Length);
                
                var lockIdentifierParameter = parameters.Single(parameter => parameter.ParameterName == ExpectedIdentifierParameterName);
                Assert.Equal(SqlDbType.Char, lockIdentifierParameter.SqlDbType);
                Assert.Equal(identifier.ToString().Length, lockIdentifierParameter.Size);
                Assert.Equal(identifier.ToString(), lockIdentifierParameter.Value);
                
                var lockIdParameter = parameters.Single(parameter => parameter.ParameterName == ExpectedIdParameterName);
                Assert.Equal(SqlDbType.Char, lockIdParameter.SqlDbType);
                Assert.Equal(id.ToString().Length, lockIdParameter.Size);
                Assert.Equal(id.ToString(), lockIdParameter.Value);
                
                var expiryDateTimeSpanInMillisecondsParameter = parameters.Single(parameter => parameter.ParameterName == ExpectedExpiryDateTimeSpanInMillisecondsParameterName);
                Assert.Equal(SqlDbType.BigInt, expiryDateTimeSpanInMillisecondsParameter.SqlDbType);
                Assert.Equal(timeToLive.TotalMilliseconds, expiryDateTimeSpanInMillisecondsParameter.Value);

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
        public async Task ReturnExpectedResult_When_TryingToUpdateAsync_And_SqlClientReturnsGivenNumberOfAffectedRows(
            bool expectedResult,
            int numberOfAffectedRows,
            string schemaName,
            string tableName,
            Guid identifier,
            Guid id,
            TimeSpan timeToLive)
        {
            SqlClientMock
                .Setup(client => client.ExecuteNonQueryAsync(
                    It.IsAny<string>(),
                    It.IsAny<SqlParameter[]>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(numberOfAffectedRows);
            
            var result = await SqlDistributedLocksTable.TryUpdateAsync(
                schemaName,
                tableName,
                identifier,
                id,
                timeToLive,
                CancellationToken.None);
            
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [AutoMoqWithInlineData(-1)]
        [AutoMoqWithInlineData(2)]
        public async Task Throw_When_TryingToUpdateAsync_And_SqlClientReturnsUnexpectedNumberOfAffectedRows(
            int numberOfAffectedRows,
            string schemaName,
            string tableName,
            Guid identifier,
            Guid id,
            TimeSpan timeToLive)
        {
            SqlClientMock
                .Setup(client => client.ExecuteNonQueryAsync(
                    It.IsAny<string>(),
                    It.IsAny<SqlParameter[]>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(numberOfAffectedRows);

            await Assert.ThrowsAsync<InvalidOperationException>(() => SqlDistributedLocksTable.TryUpdateAsync(
                schemaName,
                tableName,
                identifier,
                id,
                timeToLive,
                CancellationToken.None));
        }

        private static string GetExpectedInsertIfNotExistsCommandText(string schemaName, string tableName) =>
            "BEGIN TRANSACTION; " +
            "   SET NOCOUNT ON; " +
            "   DELETE" + 
            $"       FROM [{schemaName}].[{tableName}] " +
            $"      WHERE Identifier = {ExpectedIdentifierParameterName} AND ExpiryDateTimestamp < SYSUTCDATETIME(); " +
            "   SET NOCOUNT OFF; " +
            "COMMIT TRANSACTION; " +
            "BEGIN TRANSACTION; " +
            $"   INSERT INTO [{schemaName}].[{tableName}] " +
            "       SELECT " +
            $"          {ExpectedIdentifierParameterName}," +
            $"          {ExpectedIdParameterName}," +
            $"          DATEADD(millisecond,{ExpectedExpiryDateTimeSpanInMillisecondsParameterName},SYSUTCDATETIME()) " +
            "       WHERE" +
            "           NOT EXISTS " +
            "           (SELECT *" +
            $"           FROM [{schemaName}].[{tableName}] WITH (UPDLOCK, HOLDLOCK)" +
            $"          WHERE  Identifier = {ExpectedIdentifierParameterName}" +
            "           AND    ExpiryDateTimestamp > SYSUTCDATETIME()); " +
            "COMMIT TRANSACTION; ";

        private static string GetExpectedDeleteCommandText(string schemaName, string tableName) =>
            "DELETE " +
            $"   FROM [{schemaName}].[{tableName}] " +
            $"  WHERE   Identifier = {ExpectedIdentifierParameterName}" +
            $"  AND     Id = {ExpectedIdParameterName} " +
            "   AND     ExpiryDateTimestamp > SYSUTCDATETIME();";

        private static string GetExpectedUpdateCommandText(string schemaName, string tableName) =>
            "BEGIN TRANSACTION; " +
            $"   UPDATE [{schemaName}].[{tableName}] " +
            "   SET" +
            $"      ExpiryDateTimestamp = DATEADD(millisecond,{ExpectedExpiryDateTimeSpanInMillisecondsParameterName},SYSUTCDATETIME()) " +
            $"  WHERE   Identifier = {ExpectedIdentifierParameterName}" +
            $"  AND     Id = {ExpectedIdParameterName} " +
            "   AND     EXISTS " +
            "       (SELECT *" +
            $"       FROM [{schemaName}].[{tableName}] WITH (UPDLOCK, HOLDLOCK)" +
            $"      WHERE  Id = {ExpectedIdParameterName} " +
            "       AND    ExpiryDateTimestamp > SYSUTCDATETIME()); " +
            "COMMIT TRANSACTION; ";
    }
}