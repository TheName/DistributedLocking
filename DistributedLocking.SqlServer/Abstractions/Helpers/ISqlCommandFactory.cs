using System.Data.Common;

namespace TheName.DistributedLocking.SqlServer.Abstractions.Helpers
{
    internal interface ISqlConnectionFactory
    {
        DbConnection Create();
    }
}