using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using TheName.DistributedLocking.SqlServer.Abstractions.Configuration;
using TheName.DistributedLocking.SqlServer.Abstractions.Helpers;

namespace TheName.DistributedLocking.SqlServer.Helpers
{
    internal class SqlClient : ISqlClient
    {
        private readonly ISqlServerDistributedLockConfiguration _configuration;

        public SqlClient(ISqlServerDistributedLockConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }
        
        public async Task<T> ExecuteScalarAsync<T>(string sqlCommandText, CancellationToken cancellationToken)
        {
            var result = await ExecuteScalarAsync(sqlCommandText, cancellationToken).ConfigureAwait(false);
            if (result == null)
            {
                return default;
            }

            return (T) result;
        }

        public async Task<int> ExecuteNonQueryAsync(string sqlCommandText, SqlParameter[] sqlParameters, CancellationToken cancellationToken)
        {
            var connection = new SqlConnection(_configuration.ConnectionString);
            await using (connection.ConfigureAwait(false))
            {
                var command = new SqlCommand(sqlCommandText, connection);
                command.Parameters.AddRange(sqlParameters);
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                return await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task<object> ExecuteScalarAsync(string sqlCommandText, CancellationToken cancellationToken)
        {
            var connection = new SqlConnection(_configuration.ConnectionString);
            await using (connection.ConfigureAwait(false))
            {
                var command = new SqlCommand(sqlCommandText, connection);
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                return await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}