using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DistributedLocking.Abstractions.Repositories.Migrations;

namespace DistributedLocking.Repositories.Cassandra.Migrations
{
    internal class CassandraDistributedLocksRepositoryMigrationsProvider : IDistributedLocksRepositoryMigrationsProvider
    {
        private const string ScriptResourceNamePrefix = "DistributedLocking.Repositories.Cassandra.Migrations.Scripts.";
        private const string NamespaceGuidString = "196BE44A-D3B9-4B22-BEBE-EA5DE8D18A0E";
        private static readonly Guid NamespaceGuid = Guid.Parse(NamespaceGuidString);
        private readonly string _cassandraKeyspace;

        public CassandraDistributedLocksRepositoryMigrationsProvider(string cassandraKeyspace)
        {
            _cassandraKeyspace = cassandraKeyspace;
        }
        
        public async Task<IReadOnlyCollection<MigrationScript>> GetMigrationsAsync()
        {
            var currentAssembly = typeof(CassandraDistributedLocksRepositoryMigrationsProvider).Assembly;
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
                        content = content.Replace("{{TABLE_ID}}", GenerateTableId(_cassandraKeyspace, resourceName).ToString());
                        var script = new MigrationScript(
                            resourceName,
                            content);
                        
                        result.Add(script);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Generate an unique table id for given resource name and keyspace.
        /// The reason is that when running "create table if not exists" in parallel without an unique ID
        /// might result in creating multiple tables with same name if each request would be handled by a different
        /// node. That in turn would result in schema disagreement.
        /// We create table with an unique id which allows us to avoid this issue. 
        /// </summary>
        /// <param name="keyspace">
        /// Cassandra keyspace; we want table to have an unique id only within a keyspace.
        /// </param>
        /// <param name="resourceName">
        /// The resource name (script file name); we assume that one script file would only create one table.
        /// </param>
        /// <returns>
        /// A unique, deterministically created ID.
        /// </returns>
        private static Guid GenerateTableId(string keyspace, string resourceName)
        {
            return GenerateGuid(NamespaceGuid, $"{keyspace}-{resourceName}");
        }

        /// <summary>
        /// Generates name-based Guid using algorithm from RFC 4122 4.3 (version 5 using SHA-1)
        /// </summary>
        /// <remarks>
        /// Algorithm description: https://tools.ietf.org/html/rfc4122#section-4.3
        /// Implementation based on: https://github.com/StephenCleary/Guids/blob/master/src/Nito.Guids/GuidFactory.cs
        /// </remarks>
        private static Guid GenerateGuid(Guid namespaceGuid, string name)
        {
            const int version5 = 5;
            var namespaceBytes = ToNetworkByteOrder(NamespaceGuid.ToByteArray());
            var keyspaceAndResourceBytes = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(name);
            byte[] hashedValue;
            using (var algorithm = SHA1.Create())
            {
                algorithm.TransformBlock(namespaceBytes, 0, namespaceBytes.Length, null, 0);
                algorithm.TransformFinalBlock(keyspaceAndResourceBytes, 0, keyspaceAndResourceBytes.Length);
                hashedValue = algorithm.Hash;
            }
            
            var guidBytes = new byte[16];
            Array.Copy(hashedValue, 0, guidBytes, 0, 16);
            ToNetworkByteOrder(guidBytes);
            
            // Variant RFC4122
            guidBytes[8] = (byte)((guidBytes[8] & 0x3F) | 0x80); // big-endian octet 8

            // Version
            guidBytes[7] = (byte)((guidBytes[7] & 0x0F) | version5 << 4); // big-endian octet 6

            return new Guid(guidBytes);
        }

        private static byte[] ToNetworkByteOrder(byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                EndianSwap(bytes);
            }

            return bytes;
        }
        
        private static void EndianSwap(byte[] guid)
        {
            Swap(guid, 0, 3);
            Swap(guid, 1, 2);

            Swap(guid, 4, 5);

            Swap(guid, 6, 7);
        }

        private static void Swap(byte[] array, int index1, int index2)
        {
            var temp = array[index1];
            array[index1] = array[index2];
            array[index2] = temp;
        }
    }
}