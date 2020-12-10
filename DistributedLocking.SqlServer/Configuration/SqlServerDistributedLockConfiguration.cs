using TheName.DistributedLocking.SqlServer.Abstractions.Configuration;

namespace TheName.DistributedLocking.SqlServer.Configuration
{
    public class SqlServerDistributedLockConfiguration : ISqlServerDistributedLockConfiguration 
    {
        public string ConnectionString { get; set; }
    }
}