using DistributedLocking.Abstractions.Repositories;

namespace DistributedLocking.Abstractions.Factories
{
    public interface IDistributedLockRepositoryFactory
    {
        IDistributedLockRepository Create();
    }
}