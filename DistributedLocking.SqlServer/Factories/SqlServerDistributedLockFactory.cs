using System;
using TheName.DistributedLocking.Abstractions.Factories;
using TheName.DistributedLocking.Abstractions.Managers;
using TheName.DistributedLocking.Abstractions.Repositories;
using TheName.DistributedLocking.SqlServer.Abstractions.Configuration;
using TheName.DistributedLocking.SqlServer.Helpers;
using TheName.DistributedLocking.SqlServer.Managers;
using TheName.DistributedLocking.SqlServer.Repositories;

namespace TheName.DistributedLocking.SqlServer.Factories
{
    public class SqlServerDistributedLockFactory : 
        IDistributedLockRepositoryFactory,
        IDistributedLockRepositoryManagerFactory
    {
        private readonly ISqlServerDistributedLockConfiguration _sqlConfiguration;

        private SqlClient SqlClient => new(new SqlConnectionFactory(_sqlConfiguration));

        public SqlServerDistributedLockFactory(ISqlServerDistributedLockConfiguration sqlConfiguration)
        {
            _sqlConfiguration = sqlConfiguration ?? throw new ArgumentNullException(nameof(sqlConfiguration));
        }

        IDistributedLockRepository IDistributedLockRepositoryFactory.Create() =>
            new SqlServerDistributedLockRepository(
                new SqlDistributedLocksTable(SqlClient), _sqlConfiguration);

        IDistributedLockRepositoryManager IDistributedLockRepositoryManagerFactory.Create() =>
            new SqlServerDistributedLockRepositoryManager(
                new SqlDistributedLocksTableManager(
                    new SqlDataDefinitionLanguageExecutor(SqlClient)), _sqlConfiguration);
    }
}