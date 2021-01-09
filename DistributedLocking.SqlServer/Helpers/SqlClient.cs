using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.SqlServer.Abstractions.Helpers;

namespace DistributedLocking.SqlServer.Helpers
{
    internal class SqlClient : ISqlClient
    {
        private readonly ISqlConnectionFactory _sqlConnectionFactory;

        public SqlClient(ISqlConnectionFactory sqlConnectionFactory)
        {
            _sqlConnectionFactory = sqlConnectionFactory ?? throw new ArgumentNullException(nameof(sqlConnectionFactory));
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
            var connection = _sqlConnectionFactory.Create();
            await using (connection.ConfigureAwait(false))
            {
                var command = connection.CreateCommand();
                command.CommandText = sqlCommandText;
                command.Parameters.AddRange(sqlParameters);
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                return await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task<object> ExecuteScalarAsync(string sqlCommandText, CancellationToken cancellationToken)
        {
            var connection = _sqlConnectionFactory.Create();
            await using (connection.ConfigureAwait(false))
            {
                var command = connection.CreateCommand();
                command.CommandText = sqlCommandText;
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                return await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}