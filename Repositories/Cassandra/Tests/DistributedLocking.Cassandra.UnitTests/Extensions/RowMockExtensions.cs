using Cassandra;
using Moq;

namespace DistributedLocking.Cassandra.UnitTests.Extensions
{
    internal static class RowMockExtensions
    {
        public static Mock<Row> SetAppliedColumnValue(this Mock<Row> rowMock, bool value) => 
            rowMock.SetColumnValue(value, "[applied]");

        private static Mock<Row> SetColumnValue<T>(this Mock<Row> rowMock, T value, string columnName)
        {
            rowMock
                .Setup(row => row.GetValue<T>(columnName))
                .Returns(value);

            return rowMock;
        }
    }
}