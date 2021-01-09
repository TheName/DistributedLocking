using System.Data.Common;

namespace DistributedLocking.SqlServer.Abstractions.Helpers
{
    internal interface ISqlConnectionFactory
    {
        DbConnection Create();
    }
}