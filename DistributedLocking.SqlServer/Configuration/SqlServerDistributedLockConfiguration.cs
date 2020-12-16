using System;
using TheName.DistributedLocking.SqlServer.Abstractions.Configuration;

namespace TheName.DistributedLocking.SqlServer.Configuration
{
    internal class SqlServerDistributedLockConfiguration : ISqlServerDistributedLockConfiguration
    {
        private const string DefaultSchemaName = "TheName_DistributedLocking_SqlServer";
        private const string DefaultTableName = "DistributedLocks";
        private static readonly TimeSpan DefaultSqlApplicationLockTimeout = TimeSpan.FromSeconds(3);  
        
        public string ConnectionString { get; set; }
        public string SchemaName { get; set; } = DefaultSchemaName;
        public string TableName { get; set; } = DefaultTableName;
        public TimeSpan SqlApplicationLockTimeout { get; set; } = DefaultSqlApplicationLockTimeout;
    }
}