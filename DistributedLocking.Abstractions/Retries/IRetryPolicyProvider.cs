namespace DistributedLocking.Abstractions.Retries
{
    public interface IRetryPolicyProvider
    {
        IRetryPolicy CreateNew();
    }
}