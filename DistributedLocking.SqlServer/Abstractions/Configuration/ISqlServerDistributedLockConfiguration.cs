using System;

namespace TheName.DistributedLocking.SqlServer.Abstractions.Configuration
{
    public interface ISqlServerDistributedLockConfiguration
    {
        string ConnectionString { get; }
        string SchemaName { get; }
        string TableName { get; }
        TimeSpan SqlApplicationLockTimeout { get; }
    }
}