using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using TestHelpers.Attributes;
using TheName.DistributedLocking.SqlServer.Abstractions.Helpers;
using TheName.DistributedLocking.SqlServer.Helpers;
using Xunit;

namespace DistributedLocking.SqlServer.UnitTests.Helpers
{
    public class SqlDistributedLocksTableManager_Should
    {
        private Mock<ISqlDataDefinitionLanguageExecutor> SqlDataDefinitionLanguageExecutorMock { get; } = new();

        private SqlDistributedLocksTableManager SqlDistributedLocksTableManager => new(SqlDataDefinitionLanguageExecutorMock.Object);
        
        [Fact]
        public void Throw_When_Creating_And_SqlDataDefinitionLanguageExecutorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlDistributedLocksTableManager(null));
        }
        
        [Fact]
        public void NotThrow_When_Creating_And_SqlDataDefinitionLanguageExecutorIsNotNull()
        {
            _ = new SqlDistributedLocksTableManager(SqlDataDefinitionLanguageExecutorMock.Object);
        }

        [Theory]
        [AutoMoqWithInlineData(true)]
        [AutoMoqWithInlineData(false)]
        public async Task ReturnSqlDataDefinitionLanguageExecutorResult_When_CallingTableExistsAsync(
            bool sqlDataDefinitionLanguageExecutorResult,
            string schemaName,
            string tableName)
        {
            SqlDataDefinitionLanguageExecutorMock
                .Setup(executor => executor.TableExistsAsync(schemaName, tableName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(sqlDataDefinitionLanguageExecutorResult);

            var result = await SqlDistributedLocksTableManager.TableExistsAsync(
                schemaName,
                tableName,
                CancellationToken.None);

            Assert.Equal(sqlDataDefinitionLanguageExecutorResult, result);
        }

        [Theory]
        [AutoMoqData]
        public async Task CallSqlDataDefinitionLanguageExecutorWithExpectedCreateTableCommandText_When_CallingCreateTableIfNotExistsAsync(
            string schemaName,
            string tableName,
            TimeSpan sqlApplicationLockTimeout)
        {
            await SqlDistributedLocksTableManager.CreateTableIfNotExistsAsync(
                schemaName,
                tableName,
                sqlApplicationLockTimeout,
                CancellationToken.None);

            var expectedCreateTableCommand = GetExpectedCreateTableCommandText(schemaName, tableName);
            SqlDataDefinitionLanguageExecutorMock
                .Verify(executor => executor.CreateTableIfNotExistsAsync(
                        schemaName,
                        tableName,
                        sqlApplicationLockTimeout,
                        expectedCreateTableCommand,
                        It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        private static string GetExpectedCreateTableCommandText(string schemaName, string tableName) =>
            $"CREATE TABLE [{schemaName}].{tableName} ( " +
            "   LockIdentifier          CHAR(36)    NOT NULL UNIQUE, " +
            "   LockId                  CHAR(36)    NOT NULL UNIQUE, " +
            "   ExpiryDateTimestamp     DATETIME2   NOT NULL, " +
            $"   CONSTRAINT PK_{tableName}       PRIMARY KEY (LockIdentifier)); " +
            $"CREATE INDEX IDX_LockIdentifier_ExpiryDateTimestamp ON [{schemaName}].{tableName} (LockIdentifier, ExpiryDateTimestamp); " +
            $"CREATE INDEX IDX_LockId_ExpiryDateTimestamp ON [{schemaName}].{tableName} (LockId, ExpiryDateTimestamp); ";
    }
}