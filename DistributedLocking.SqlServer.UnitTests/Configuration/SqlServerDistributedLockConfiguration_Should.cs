using System;
using DistributedLocking.SqlServer.Configuration;
using Xunit;

namespace DistributedLocking.SqlServer.UnitTests.Configuration
{
    public class SqlServerDistributedLockConfiguration_Should
    {
        [Fact]
        public void PopulateSchemaNameWithDefault()
        {
            const string expectedDefaultSchemaName = "TheName_DistributedLocking_SqlServer";
            var defaultConfiguration = new SqlServerDistributedLockConfiguration();
            
            Assert.Equal(expectedDefaultSchemaName, defaultConfiguration.SchemaName);
        }
        
        [Fact]
        public void PopulateTableNameWithDefault()
        {
            const string expectedDefaultTableName = "DistributedLocks";
            var defaultConfiguration = new SqlServerDistributedLockConfiguration();
            
            Assert.Equal(expectedDefaultTableName, defaultConfiguration.TableName);
        }
        
        [Fact]
        public void PopulateSqlApplicationLockTimeoutWithDefault()
        {
            var expectedDefaultSqlApplicationLockTimeout = TimeSpan.FromSeconds(3);
            var defaultConfiguration = new SqlServerDistributedLockConfiguration();
            
            Assert.Equal(expectedDefaultSqlApplicationLockTimeout, defaultConfiguration.SqlApplicationLockTimeout);
        }
    }
}