using DistributedLocking.Abstractions.Managers;

namespace DistributedLocking.Abstractions.Factories
{
    public interface IDistributedLockRepositoryManagerFactory
    {
        IDistributedLockRepositoryManager Create();
    }
}