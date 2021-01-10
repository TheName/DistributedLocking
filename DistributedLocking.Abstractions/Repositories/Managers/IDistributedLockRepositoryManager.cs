using System.Threading;
using System.Threading.Tasks;

namespace DistributedLocking.Abstractions.Repositories.Managers
{
    public interface IDistributedLockRepositoryManager
    {
        Task<bool> RepositoryExistsAsync(CancellationToken cancellationToken);

        Task CreateIfNotExistsAsync(CancellationToken cancellationToken);
    }
}