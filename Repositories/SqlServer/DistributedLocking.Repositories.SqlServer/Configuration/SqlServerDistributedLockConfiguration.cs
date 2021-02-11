using DistributedLocking.Repositories.SqlServer.Abstractions.Configuration;

namespace DistributedLocking.Repositories.SqlServer.Configuration
{
    internal class SqlServerDistributedLockConfiguration : ISqlServerDistributedLockConfiguration
    {
        public string ConnectionString { get; set; }
    }
}