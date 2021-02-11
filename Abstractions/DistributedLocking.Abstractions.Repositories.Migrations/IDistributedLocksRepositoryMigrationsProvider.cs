using System.Collections.Generic;
using System.Threading.Tasks;

namespace DistributedLocking.Abstractions.Repositories.Migrations
{
    /// <summary>
    /// The distributed locks repository migrations provider.
    /// </summary>
    public interface IDistributedLocksRepositoryMigrationsProvider
    {
        /// <summary>
        /// Gets repository migrations.
        /// </summary>
        /// <returns>
        /// A readonly collection of ordered migration scripts.
        /// </returns>
        Task<IReadOnlyCollection<MigrationScript>> GetMigrationsAsync();
    }
}