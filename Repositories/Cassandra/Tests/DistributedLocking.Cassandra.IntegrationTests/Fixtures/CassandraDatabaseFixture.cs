using System;
using System.Threading.Tasks;
using Cassandra;
using DistributedLocking.Extensions.Repositories.Cassandra.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace DistributedLocking.Cassandra.IntegrationTests.Fixtures
{
    public class CassandraDatabaseFixture : IDisposable
    {
        private IServiceProvider ServiceProvider { get; }

        public string Keyspace { get; }
        
        public CassandraDatabaseFixture()
        {
            Keyspace = Guid.NewGuid().ToString().Replace("-", "_");
            var cluster = Cluster
                .Builder()
                .AddContactPoint("localhost")
                .WithDefaultKeyspace(Keyspace)
                .Build();
            var session = cluster.ConnectAndCreateDefaultKeyspaceIfNotExists();
            
            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddSingleton(session)
                .AddCassandraDistributedLocking();

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        public T GetService<T>() => ServiceProvider.GetRequiredService<T>();

        public void Dispose()
        {
            // DropDatabaseAsync().GetAwaiter().GetResult();
            if (ServiceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        private async Task DropDatabaseAsync()
        {
            await GetService<ISession>().ExecuteAsync(new SimpleStatement($"DROP KEYSPACE IF EXISTS \"{Keyspace}\""));
        }
    }
}