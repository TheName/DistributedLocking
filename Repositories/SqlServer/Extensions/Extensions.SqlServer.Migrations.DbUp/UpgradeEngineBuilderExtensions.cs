using DbUp.Builder;
using DistributedLocking.Repositories.Migrations.DbUp.Extensions;

namespace DistributedLocking.Repositories.SqlServer.Migrations.DbUp
{
    /// <summary>
    /// Extensions for <see cref="UpgradeEngineBuilder"/>
    /// </summary>
    public static class UpgradeEngineBuilderExtensions
    {
        /// <summary>
        /// Adds distributed locking SQL Server migrations to the builder.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="UpgradeEngineBuilder"/>.
        /// </param>
        /// <returns>
        /// The <see cref="UpgradeEngineBuilder"/>.
        /// </returns>
        public static UpgradeEngineBuilder WithDistributedLockingSqlServerMigrations(this UpgradeEngineBuilder builder) => 
            builder.WithDistributedLockingMigrations<SqlDistributedLocksRepositoryMigrationsProvider>();
    }
}