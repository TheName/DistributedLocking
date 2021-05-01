using System;
using System.Linq;
using Cassandra;

namespace DistributedLocking.Repositories.Cassandra.Extensions
{
    internal static class RowSetExtensions
    {
        public static bool IsApplied(this RowSet rowSet) =>
            (rowSet ?? throw new ArgumentNullException(nameof(rowSet))).Single().GetValue<bool>("[applied]");
    }
}