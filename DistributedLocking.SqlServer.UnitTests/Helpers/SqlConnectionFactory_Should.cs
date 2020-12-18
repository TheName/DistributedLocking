using System;
using System.Data.SqlClient;
using DistributedLocking.SqlServer.UnitTests.Extensions;
using Moq;
using TestHelpers.Attributes;
using TheName.DistributedLocking.SqlServer.Abstractions.Configuration;
using TheName.DistributedLocking.SqlServer.Helpers;
using Xunit;

namespace DistributedLocking.SqlServer.UnitTests.Helpers
{
    public class SqlConnectionFactory_Should
    {
        [Fact]
        public void Throw_When_Creating_And_SqlConnectionFactoryIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlConnectionFactory(null));
        }
        
        [Theory]
        [AutoMoqData]
        public void NotThrow_When_Creating_And_SqlConnectionFactoryIsNotNull(ISqlServerDistributedLockConfiguration configuration)
        {
            _ = new SqlConnectionFactory(configuration);
        }

        [Theory]
        [AutoMoqWithInlineData("Server=localhost;")]
        public void CreateSqlConnection(
            string sqlConnectionString,
            ISqlServerDistributedLockConfiguration configuration)
        {
            Mock.Get(configuration).SetupConnectionString(sqlConnectionString);
            
            var sqlConnectionFactory = new SqlConnectionFactory(configuration);
            var connection = sqlConnectionFactory.Create();

            Assert.IsType<SqlConnection>(connection);
            Assert.Equal(sqlConnectionString, connection.ConnectionString);
        }
    }
}