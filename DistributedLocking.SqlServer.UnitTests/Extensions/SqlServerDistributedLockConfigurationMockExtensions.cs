using System;
using DistributedLocking.SqlServer.Abstractions.Configuration;
using Moq;

namespace DistributedLocking.SqlServer.UnitTests.Extensions
{
    public static class SqlServerDistributedLockConfigurationMockExtensions
    {
        public static Mock<ISqlServerDistributedLockConfiguration> SetupSchemaName(
            this Mock<ISqlServerDistributedLockConfiguration> configurationMock,
            string schemaName)
        {
            configurationMock
                .SetupGet(configuration => configuration.SchemaName)
                .Returns(schemaName);

            return configurationMock;
        }

        public static Mock<ISqlServerDistributedLockConfiguration> SetupTableName(
            this Mock<ISqlServerDistributedLockConfiguration> configurationMock,
            string tableName)
        {
            configurationMock
                .SetupGet(configuration => configuration.TableName)
                .Returns(tableName);

            return configurationMock;
        }

        public static Mock<ISqlServerDistributedLockConfiguration> SetupSqlApplicationLockTimeout(
            this Mock<ISqlServerDistributedLockConfiguration> configurationMock,
            TimeSpan sqlApplicationLockTimeout)
        {
            configurationMock
                .SetupGet(configuration => configuration.SqlApplicationLockTimeout)
                .Returns(sqlApplicationLockTimeout);

            return configurationMock;
        }

        public static Mock<ISqlServerDistributedLockConfiguration> SetupConnectionString(
            this Mock<ISqlServerDistributedLockConfiguration> configurationMock,
            string connectionString)
        {
            configurationMock
                .SetupGet(configuration => configuration.ConnectionString)
                .Returns(connectionString);

            return configurationMock;
        }
    }
}