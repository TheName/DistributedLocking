using System.Threading;
using System.Threading.Tasks;

namespace DistributedLocking.Abstractions.Initializers
{
    public interface IDistributedLockRepositoryInitializer
    {
        Task InitializeAsync(CancellationToken cancellationToken);
    }
}