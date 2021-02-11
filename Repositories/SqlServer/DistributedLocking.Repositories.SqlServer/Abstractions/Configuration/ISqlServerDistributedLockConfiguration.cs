namespace DistributedLocking.Repositories.SqlServer.Abstractions.Configuration
{
    /// <summary>
    /// The SQL Server distributed lock configuration
    /// </summary>
    public interface ISqlServerDistributedLockConfiguration
    {
        /// <summary>
        /// The SQL Server connection string.
        /// </summary>
        string ConnectionString { get; }
    }
}