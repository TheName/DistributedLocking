using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheName.DistributedLocking.Extensions.DependencyInjection.SqlServer;
using TheName.DistributedLocking.SqlServer.Abstractions.Configuration;

namespace DistributedLocking.SqlServer.IntegrationTests.Fixtures
{
    public class SqlDatabaseFixture : IDisposable
    {
        private const string ConfigurationFileName = "appsettings.json";

        private IServiceProvider ServiceProvider { get; }

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
                .AddSqlServerDistributedLocking()
                .Decorate<ISqlServerDistributedLockConfiguration, SqlServerDistributedLockConfiguration>();

            ServiceProvider = serviceCollection.BuildServiceProvider();
            
            CreateDatabaseAsync().GetAwaiter().GetResult();
        }

        public T GetService<T>() => ServiceProvider.GetRequiredService<T>();

        public void Dispose()
        {
            DropDatabaseAsync().GetAwaiter().GetResult();
            if (ServiceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
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
        
        private class SqlServerDistributedLockConfiguration : ISqlServerDistributedLockConfiguration
        {
            private readonly ISqlServerDistributedLockConfiguration _configuration;
            private readonly Guid _databaseId = Guid.NewGuid();
            
            public string ConnectionString
            {
                get
                {
                    var sqlConnectionBuilder = new SqlConnectionStringBuilder(_configuration.ConnectionString);
                    sqlConnectionBuilder.InitialCatalog = $"{sqlConnectionBuilder.InitialCatalog}_{_databaseId}";
                    return sqlConnectionBuilder.ConnectionString;
                }
            }

            public string SchemaName => _configuration.SchemaName;
            public string TableName => _configuration.TableName;
            public TimeSpan SqlApplicationLockTimeout => _configuration.SqlApplicationLockTimeout;

            public SqlServerDistributedLockConfiguration(ISqlServerDistributedLockConfiguration configuration)
            {
                _configuration = configuration;
            }
        }
    }
}