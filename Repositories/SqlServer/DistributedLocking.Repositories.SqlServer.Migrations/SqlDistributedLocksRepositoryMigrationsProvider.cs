using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DistributedLocking.Abstractions.Repositories.Migrations;

namespace DistributedLocking.Repositories.SqlServer.Migrations
{
    internal class SqlDistributedLocksRepositoryMigrationsProvider : IDistributedLocksRepositoryMigrationsProvider
    {
        private const string ScriptResourceNamePrefix = "DistributedLocking.Repositories.SqlServer.Migrations.Scripts.";
        
        public async Task<IReadOnlyCollection<MigrationScript>> GetMigrationsAsync()
        {
            var currentAssembly = typeof(SqlDistributedLocksRepositoryMigrationsProvider).Assembly;
            var orderedResourceNames = currentAssembly
                .GetManifestResourceNames()
                .Where(name => name.StartsWith(ScriptResourceNamePrefix))
                .OrderBy(name => name);

            var result = new List<MigrationScript>();
            foreach (var resourceName in orderedResourceNames)
            {
                using (var resourceStream = currentAssembly.GetManifestResourceStream(resourceName))
                {
                    if (resourceStream == null)
                    {
                        throw new InvalidOperationException($"Could not load resource stream with name {resourceName}");
                    }

                    using (var streamReader = new StreamReader(resourceStream))
                    {
                        var content = await streamReader.ReadToEndAsync().ConfigureAwait(false);
                        var script = new MigrationScript(
                            resourceName,
                            content);
                        
                        result.Add(script);
                    }
                }
            }

            return result;
        }
    }
}