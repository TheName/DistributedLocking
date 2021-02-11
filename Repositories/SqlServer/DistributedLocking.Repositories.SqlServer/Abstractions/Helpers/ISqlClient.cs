using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedLocking.Repositories.SqlServer.Abstractions.Helpers
{
    internal interface ISqlClient
    {
        Task<int> ExecuteNonQueryAsync(
            string sqlCommandText,
            IEnumerable<SqlParameter> sqlParameters,
            CancellationToken cancellationToken);
    }
}