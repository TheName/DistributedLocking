using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.Repositories.SqlServer.Abstractions.Helpers;

namespace DistributedLocking.Repositories.SqlServer.Helpers
{
    internal class SqlClient : ISqlClient
    {
        private readonly ISqlConnectionFactory _sqlConnectionFactory;

        public SqlClient(ISqlConnectionFactory sqlConnectionFactory)
        {
            _sqlConnectionFactory = sqlConnectionFactory ?? throw new ArgumentNullException(nameof(sqlConnectionFactory));
        }

        public async Task<int> ExecuteNonQueryAsync(
            string sqlCommandText,
            IEnumerable<SqlParameter> sqlParameters,
            CancellationToken cancellationToken)
        {
            using var connection = _sqlConnectionFactory.Create();
            using var command = connection.CreateCommand();
            command.CommandText = sqlCommandText;
            command.Parameters.AddRange(sqlParameters.ToArray());
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            return await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}