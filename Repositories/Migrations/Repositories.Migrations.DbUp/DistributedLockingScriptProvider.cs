using System;
using System.Collections.Generic;
using System.Linq;
using DbUp.Engine;
using DbUp.Engine.Transactions;
using DistributedLocking.Abstractions.Repositories.Migrations;

namespace DistributedLocking.Repositories.Migrations.DbUp
{
    /// <summary>
    /// DbUp script provider for DistributedLocking package
    /// </summary>
    public class DistributedLockingScriptProvider : IScriptProvider
    {
        private readonly IDistributedLocksRepositoryMigrationsProvider _migrationsProvider;

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="migrationsProvider">
        /// The <see cref="IDistributedLocksRepositoryMigrationsProvider"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="migrationsProvider"/> is null.
        /// </exception>
        public DistributedLockingScriptProvider(IDistributedLocksRepositoryMigrationsProvider migrationsProvider)
        {
            _migrationsProvider = migrationsProvider ?? throw new ArgumentNullException(nameof(migrationsProvider));
        }

        /// <inheritdoc />
        public IEnumerable<SqlScript> GetScripts(IConnectionManager connectionManager)
        {
            var migrationScripts = _migrationsProvider.GetMigrationsAsync().GetAwaiter().GetResult();

            return migrationScripts
                .Select(script => new SqlScript(script.Name, script.Content));
        }
    }
}