using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace TheName.DistributedLocking.SqlServer.Abstractions.Helpers
{
    internal interface ISqlClient
    {
        Task<T> ExecuteScalarAsync<T>(string sqlCommandText, CancellationToken cancellationToken);

        Task<int> ExecuteNonQueryAsync(
            string sqlCommandText,
            SqlParameter[] sqlParameters,
            CancellationToken cancellationToken);
    }
}