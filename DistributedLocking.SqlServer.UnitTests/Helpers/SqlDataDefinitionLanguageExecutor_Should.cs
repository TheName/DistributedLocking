using System;
using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.SqlServer.Abstractions.Helpers;
using DistributedLocking.SqlServer.Helpers;
using Moq;
using TestHelpers.Attributes;
using Xunit;

namespace DistributedLocking.SqlServer.UnitTests.Helpers
{
    public class SqlDataDefinitionLanguageExecutor_Should
    {
        private Mock<ISqlClient> SqlClientMock { get; } = new();

        private SqlDataDefinitionLanguageExecutor SqlDataDefinitionLanguageExecutor => new(SqlClientMock.Object);
        
        [Fact]
        public void Throw_When_Creating_And_SqlClientIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlDataDefinitionLanguageExecutor(null));
        }
        
        [Fact]
        public void NotThrow_When_Creating_And_SqlClientIsNotNull()
        {
            _ = new SqlDataDefinitionLanguageExecutor(SqlClientMock.Object);
        }

        [Theory]
        [AutoMoqData]
        public async Task ReturnTrue_When_CheckingIfTableExists_And_SqlClientReturnsNotNull(
            string schemaName,
            string tableName,
            object queryResult)
        {
            var expectedCommandText = $"SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{schemaName}' AND TABLE_NAME = '{tableName}';";

            SqlClientMock
                .Setup(client => client.ExecuteScalarAsync<object>(expectedCommandText, It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);

            var result = await SqlDataDefinitionLanguageExecutor.TableExistsAsync(schemaName, tableName, CancellationToken.None);
            
            Assert.True(result);
        }

        [Theory]
        [AutoMoqData]
        public async Task ReturnFalse_When_CheckingIfTableExists_And_SqlClientReturnsNull(
            string schemaName,
            string tableName)
        {
            SqlClientMock
                .Setup(client => client.ExecuteScalarAsync<object>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(null as object);

            var result = await SqlDataDefinitionLanguageExecutor.TableExistsAsync(schemaName, tableName, CancellationToken.None);
            
            Assert.False(result);
        }

        [Theory]
        [AutoMoqData]
        public async Task CallSqlClientWithExpectedCommand_When_CreatingTableIfNotExists(
            string schemaName,
            string tableName,
            TimeSpan applicationLockTimeout,
            string createTableSqlCommandText)
        {
            var expectedCommandText = GetExpectedCreateIfNotExistsCommandText(
                schemaName,
                tableName,
                applicationLockTimeout,
                createTableSqlCommandText);
            
            SqlClientMock
                .Setup(client => client.ExecuteScalarAsync<int>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            await SqlDataDefinitionLanguageExecutor.CreateTableIfNotExistsAsync(
                schemaName,
                tableName,
                applicationLockTimeout,
                createTableSqlCommandText,
                CancellationToken.None);

            SqlClientMock
                .Verify(
                    client => client.ExecuteScalarAsync<int>(expectedCommandText, It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        [Theory]
        [AutoMoqWithInlineData(0)]
        [AutoMoqWithInlineData(1)]
        public async Task NotThrow_When_CreatingTableIfNotExists_And_SqlClientReturnsSuccessfulResult(
            int successfulResult,
            string schemaName,
            string tableName,
            TimeSpan applicationLockTimeout,
            string createTableSqlCommandText)
        {
            SqlClientMock
                .Setup(client => client.ExecuteScalarAsync<int>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(successfulResult);

            await SqlDataDefinitionLanguageExecutor.CreateTableIfNotExistsAsync(
                schemaName,
                tableName,
                applicationLockTimeout,
                createTableSqlCommandText,
                CancellationToken.None);
        }

        [Theory]
        [AutoMoqWithInlineData(-1)]
        public async Task Throw_When_CreatingTableIfNotExists_And_SqlClientReturnsTimeoutResult(
            int timeoutResult,
            string schemaName,
            string tableName,
            TimeSpan applicationLockTimeout,
            string createTableSqlCommandText)
        {
            SqlClientMock
                .Setup(client => client.ExecuteScalarAsync<int>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(timeoutResult);

            await Assert.ThrowsAsync<TimeoutException>(() =>
                SqlDataDefinitionLanguageExecutor.CreateTableIfNotExistsAsync(
                    schemaName,
                    tableName,
                    applicationLockTimeout,
                    createTableSqlCommandText,
                    CancellationToken.None));
        }

        [Theory]
        [AutoMoqWithInlineData(-2)]
        [AutoMoqWithInlineData(-3)]
        [AutoMoqWithInlineData(-999)]
        public async Task Throw_When_CreatingTableIfNotExists_And_SqlClientReturnsCancelledDeadlockOrParameterValidationOrOtherErrorResult(
            int cancelledDeadlockOrParameterValidationOrOtherErrorResult,
            string schemaName,
            string tableName,
            TimeSpan applicationLockTimeout,
            string createTableSqlCommandText)
        {
            SqlClientMock
                .Setup(client => client.ExecuteScalarAsync<int>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(cancelledDeadlockOrParameterValidationOrOtherErrorResult);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                SqlDataDefinitionLanguageExecutor.CreateTableIfNotExistsAsync(
                    schemaName,
                    tableName,
                    applicationLockTimeout,
                    createTableSqlCommandText,
                    CancellationToken.None));
        }

        [Theory]
        [AutoMoqWithInlineData(-4)]
        [AutoMoqWithInlineData(2)]
        public async Task Throw_When_CreatingTableIfNotExists_And_SqlClientReturnsNotExpectedResult(
            int notExpectedResult,
            string schemaName,
            string tableName,
            TimeSpan applicationLockTimeout,
            string createTableSqlCommandText)
        {
            SqlClientMock
                .Setup(client => client.ExecuteScalarAsync<int>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(notExpectedResult);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                SqlDataDefinitionLanguageExecutor.CreateTableIfNotExistsAsync(
                    schemaName,
                    tableName,
                    applicationLockTimeout,
                    createTableSqlCommandText,
                    CancellationToken.None));
        }

        private static string GetExpectedCreateIfNotExistsCommandText(
            string schemaName,
            string tableName,
            TimeSpan applicationLockTimeout,
            string createTableSqlCommandText) =>
            "BEGIN TRANSACTION; " +
            "DECLARE @getapplock_result int; " +
            $"EXEC @getapplock_result = sp_getapplock @Resource = '{schemaName}', @LockMode = 'Exclusive', @LockTimeout = {(int)applicationLockTimeout.TotalMilliseconds}; " +
            "IF @getapplock_result < 0 " +
            "BEGIN" +
            "   ROLLBACK TRANSACTION;" +
            "   SELECT @getapplock_result;" +
            "END " +
            "ELSE " +
            "BEGIN " +
            $"  IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{schemaName}')) " +
            "   BEGIN " +
            $"      EXEC ('CREATE SCHEMA [{schemaName}];') " +
            "   END " +
            $"  IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{schemaName}' AND TABLE_NAME = '{tableName}')) " +
            "   BEGIN" +
            $"       {createTableSqlCommandText} " +
            "       COMMIT TRANSACTION;  " +
            "       SELECT @getapplock_result;" +
            "   END " +
            "END; ";
    }
}