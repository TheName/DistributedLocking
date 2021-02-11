using System;
using System.Data.Common;
using System.Data.SqlClient;
using DistributedLocking.Repositories.SqlServer.Abstractions.Configuration;
using DistributedLocking.Repositories.SqlServer.Abstractions.Helpers;

namespace DistributedLocking.Repositories.SqlServer.Helpers
{
    internal class SqlConnectionFactory : ISqlConnectionFactory
    {
        private readonly ISqlServerDistributedLockConfiguration _configuration;

        public SqlConnectionFactory(ISqlServerDistributedLockConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public DbConnection Create() => new SqlConnection(_configuration.ConnectionString);
    }
}