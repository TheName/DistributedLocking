using System.Collections.Generic;
using Cassandra;
using Moq;

namespace DistributedLocking.Cassandra.UnitTests.Extensions
{
    internal static class RowSetMockExtensions
    {
        public static Mock<RowSet> SetupEnumeratorWithSingleRowAndAppliedColumnValue(this Mock<RowSet> rowSetMock, bool value) =>
            rowSetMock.SetupEnumerator(new Mock<Row>().SetAppliedColumnValue(value).Object);

        public static Mock<RowSet> SetupEnumerator(this Mock<RowSet> rowSetMock, Row next = null, bool hasMore = false)
        {
            var enumeratorMock = new Mock<IEnumerator<Row>>();
            enumeratorMock
                .SetupSequence(enumerator => enumerator.MoveNext())
                .Returns(next != null)
                .Returns(hasMore);

            if (next != null)
            {
                enumeratorMock
                    .Setup(enumerator => enumerator.Current)
                    .Returns(next);
            }
            
            rowSetMock
                .Setup(set => set.GetEnumerator())
                .Returns(enumeratorMock.Object);

            return rowSetMock;
        }
    }
}