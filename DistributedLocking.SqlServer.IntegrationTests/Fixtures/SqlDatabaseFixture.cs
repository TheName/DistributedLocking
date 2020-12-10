using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TheName.DistributedLocking.Abstractions.Repositories;
using TheName.DistributedLocking.SqlServer.Abstractions.Configuration;
using TheName.DistributedLocking.SqlServer.Configuration;
using TheName.DistributedLocking.SqlServer.Repositories;

namespace DistributedLocking.SqlServer.IntegrationTests.Fixtures
{
    public class SqlDatabaseFixture : IDisposable
    {
        private const string ConfigurationFileName = "appsettings.json";
        
        public IServiceProvider ServiceProvider { get; }
        
        private string DatabaseName 
        {
            get
            {
                var sqlConfiguration = ServiceProvider.GetRequiredService<ISqlServerDistributedLockConfiguration>();
                var sqlConnectionBuilder = new SqlConnectionStringBuilder(sqlConfiguration.ConnectionString);
                return sqlConnectionBuilder.InitialCatalog;
            }
        }
        
        public SqlDatabaseFixture()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(ConfigurationFileName, false, false)
                .Build();

            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddSingleton<IConfiguration>(configuration)
                .AddOptions<SqlServerDistributedLockConfiguration>()
                .Bind(configuration.GetSection(nameof(SqlServerDistributedLockConfiguration)))
                .PostConfigure(lockConfiguration =>
                {
                    var sqlConnectionBuilder = new SqlConnectionStringBuilder(lockConfiguration.ConnectionString);
                    sqlConnectionBuilder.InitialCatalog = $"{sqlConnectionBuilder.InitialCatalog}_{Guid.NewGuid()}";
                    lockConfiguration.ConnectionString = sqlConnectionBuilder.ConnectionString;
                });

            serviceCollection
                .AddSingleton<ISqlServerDistributedLockConfiguration>(provider =>
                    provider.GetRequiredService<IOptions<SqlServerDistributedLockConfiguration>>().Value)
                .AddSingleton<IDistributedLockRepository, SqlServerDistributedLockRepository>();

            ServiceProvider = serviceCollection.BuildServiceProvider();
            
            CreateDatabaseAsync().GetAwaiter().GetResult();
        }

        private async Task CreateDatabaseAsync() =>
            await ExecuteNonQueryAsyncWithoutInitialCatalog($"CREATE DATABASE \"{DatabaseName}\"");

        private async Task DropDatabaseAsync()
        {
            SqlConnection.ClearAllPools();
            await ExecuteNonQueryAsyncWithoutInitialCatalog($"DROP DATABASE \"{DatabaseName}\"");
        }

        private async Task ExecuteNonQueryAsyncWithoutInitialCatalog(string sqlCommand)
        {
            var sqlConfiguration = ServiceProvider.GetRequiredService<ISqlServerDistributedLockConfiguration>();
            var sqlConnectionBuilder = new SqlConnectionStringBuilder(sqlConfiguration.ConnectionString)
            {
                InitialCatalog = string.Empty
            };

            await using var connection = new SqlConnection(sqlConnectionBuilder.ConnectionString);
            var command = new SqlCommand(sqlCommand, connection);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }
        
        public void Dispose()
        {
            DropDatabaseAsync().GetAwaiter().GetResult();
            if (ServiceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}