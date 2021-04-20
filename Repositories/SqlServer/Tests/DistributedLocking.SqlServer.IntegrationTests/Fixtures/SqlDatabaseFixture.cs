using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using DistributedLocking.Extensions.Repositories.SqlServer.DependencyInjection;
using DistributedLocking.Repositories.SqlServer.Abstractions.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DistributedLocking.SqlServer.IntegrationTests.Fixtures
{
    public class SqlDatabaseFixture : IDisposable
    {
        private const string ConfigurationFileName = "appsettings.json";

        private readonly string _connectionString;

        private IServiceProvider ServiceProvider { get; }

        private string DatabaseName 
        {
            get
            {
                var sqlConnectionBuilder = new SqlConnectionStringBuilder(_connectionString);
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

            var sqlConfiguration = ServiceProvider.GetRequiredService<ISqlServerDistributedLockConfiguration>();
            _connectionString = sqlConfiguration.ConnectionString;
            
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
            await ExecuteNonQueryAsyncWithoutInitialCatalog($"ALTER DATABASE \"{DatabaseName}\" " +
                                                            "SET SINGLE_USER " +
                                                            "WITH ROLLBACK IMMEDIATE; " +
                                                            $"DROP DATABASE \"{DatabaseName}\"");
        }

        private async Task ExecuteNonQueryAsyncWithoutInitialCatalog(string sqlCommand)
        {
            var sqlConnectionBuilder = new SqlConnectionStringBuilder(_connectionString)
            {
                InitialCatalog = string.Empty
            };

            try
            {
                await using var connection = new SqlConnection(sqlConnectionBuilder.ConnectionString);
                var command = new SqlCommand(sqlCommand, connection);
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                throw new Exception(
                    $"Could not execute SQL command. Connection string: \"{sqlConnectionBuilder.ConnectionString}\", SQL command: \"{sqlCommand}\"",
                    e);
            }
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

            public SqlServerDistributedLockConfiguration(ISqlServerDistributedLockConfiguration configuration)
            {
                _configuration = configuration;
            }
        }
    }
}