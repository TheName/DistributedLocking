using DbUp.Builder;
using DistributedLocking.Abstractions.Repositories.Migrations;

namespace DistributedLocking.Repositories.Migrations.DbUp.Extensions
{
    /// <summary>
    /// Extensions for <see cref="UpgradeEngineBuilder"/>
    /// </summary>
    public static class UpgradeEngineBuilderExtensions
    {
        /// <summary>
        /// Adds distributed locking script provider with provided migrations provider type to the builder.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="UpgradeEngineBuilder"/>.
        /// </param>
        /// <typeparam name="TMigrationsProvider">
        /// The <see cref="IDistributedLocksRepositoryMigrationsProvider"/> type that should be used as migrations source.
        /// </typeparam>
        /// <returns>
        /// The <see cref="UpgradeEngineBuilder"/>.
        /// </returns>
        public static UpgradeEngineBuilder WithDistributedLockingMigrations<TMigrationsProvider>(this UpgradeEngineBuilder builder)
            where TMigrationsProvider : IDistributedLocksRepositoryMigrationsProvider, new() =>
            builder.WithDistributedLockingMigrations(new TMigrationsProvider());

        /// <summary>
        /// Adds distributed locking script provider with provided migrations provider to the builder.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="UpgradeEngineBuilder"/>.
        /// </param>
        /// <param name="migrationsProvider">
        /// The <see cref="IDistributedLocksRepositoryMigrationsProvider"/>.
        /// </param>
        /// <returns>
        /// The <see cref="UpgradeEngineBuilder"/>.
        /// </returns>
        public static UpgradeEngineBuilder WithDistributedLockingMigrations(
            this UpgradeEngineBuilder builder,
            IDistributedLocksRepositoryMigrationsProvider migrationsProvider) =>
            builder.WithScripts(new DistributedLockingScriptProvider(migrationsProvider));
    }
}