using System.Data.Common;

namespace DistributedLocking.Repositories.SqlServer.Abstractions.Helpers
{
    internal interface ISqlConnectionFactory
    {
        DbConnection Create();
    }
}