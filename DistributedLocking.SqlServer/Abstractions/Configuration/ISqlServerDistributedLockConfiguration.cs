namespace TheName.DistributedLocking.SqlServer.Abstractions.Configuration
{
    public interface ISqlServerDistributedLockConfiguration
    {
        string ConnectionString { get; }
    }
}