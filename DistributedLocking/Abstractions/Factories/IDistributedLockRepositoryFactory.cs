using TheName.DistributedLocking.Abstractions.Repositories;

namespace TheName.DistributedLocking.Abstractions.Factories
{
    public interface IDistributedLockRepositoryFactory
    {
        IDistributedLockRepository Create();
    }
}