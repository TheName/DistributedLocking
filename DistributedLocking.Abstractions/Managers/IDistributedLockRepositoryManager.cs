using System.Threading;
using System.Threading.Tasks;

namespace DistributedLocking.Abstractions.Managers
{
    public interface IDistributedLockRepositoryManager
    {
        Task<bool> RepositoryExistsAsync(CancellationToken cancellationToken);

        Task CreateIfNotExistsAsync(CancellationToken cancellationToken);
    }
}