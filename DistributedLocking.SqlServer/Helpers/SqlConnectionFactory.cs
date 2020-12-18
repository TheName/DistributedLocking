using System;
using System.Data.Common;
using System.Data.SqlClient;
using TheName.DistributedLocking.SqlServer.Abstractions.Configuration;
using TheName.DistributedLocking.SqlServer.Abstractions.Helpers;

namespace TheName.DistributedLocking.SqlServer.Helpers
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