using System;
using DistributedLocking.Abstractions.Factories;
using DistributedLocking.Abstractions.Managers;
using DistributedLocking.Abstractions.Repositories;
using DistributedLocking.SqlServer.Abstractions.Configuration;
using DistributedLocking.SqlServer.Helpers;
using DistributedLocking.SqlServer.Managers;
using DistributedLocking.SqlServer.Repositories;

namespace DistributedLocking.SqlServer.Factories
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