using TheName.DistributedLocking.Abstractions.Managers;

namespace TheName.DistributedLocking.Abstractions.Factories
{
    public interface IDistributedLockRepositoryManagerFactory
    {
        IDistributedLockRepositoryManager Create();
    }
}