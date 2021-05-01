using System;
using Cassandra;
using DistributedLocking.Repositories.Cassandra.Extensions;
using Moq;
using TestHelpers.Attributes;
using Xunit;

namespace DistributedLocking.Cassandra.UnitTests.Extensions
{
    public class RowSetExtensions_Should
    {
        [Fact]
        public void Throw_When_CallingIsApplied_And_RowSetIsNull()
        {
            RowSet rowSet = null;
            Assert.Throws<ArgumentNullException>(() => rowSet.IsApplied());
        }
        
        [Theory]
        [AutoMoqData]
        public void Throw_When_CallingIsApplied_And_RowSetContainsNoElements(Mock<RowSet> rowSetMock)
        {
            rowSetMock.SetupEnumerator();
            var rowSet = rowSetMock.Object;

            Assert.Throws<InvalidOperationException>(() => rowSet.IsApplied());
        }
        
        [Theory]
        [AutoMoqData]
        public void Throw_When_CallingIsApplied_And_RowSetContainsMoreThanOneElement(
            Mock<RowSet> rowSetMock,
            Row row)
        {
            rowSetMock.SetupEnumerator(row, true);
            var rowSet = rowSetMock.Object;

            Assert.Throws<InvalidOperationException>(() => rowSet.IsApplied());
        }
        
        [Theory]
        [AutoMoqData]
        public void Throw_When_CallingIsApplied_And_RowSetContainsOneElement_And_RowDoesNotContainAppliedColumn(
            Mock<RowSet> rowSetMock,
            Mock<Row> rowMock)
        {
            rowMock
                .Setup(row => row.GetValue<bool>(It.IsAny<string>()))
                .Throws<ArgumentException>();
            
            rowSetMock.SetupEnumerator(rowMock.Object);
            var rowSet = rowSetMock.Object;

            Assert.Throws<ArgumentException>(() => rowSet.IsApplied());
        }
        
        [Theory]
        [AutoMoqWithInlineData(true)]
        [AutoMoqWithInlineData(false)]
        public void ReturnResult_When_CallingIsApplied_And_RowSetContainsOneElement_And_RowDoesContainAppliedColumn_And_ItIsNotParsableToBoolean(
            bool applied,
            Mock<RowSet> rowSetMock)
        {
            rowSetMock.SetupEnumeratorWithSingleRowAndAppliedColumnValue(applied);
            var rowSet = rowSetMock.Object;

            var result = rowSet.IsApplied();

            Assert.Equal(applied, result);
        }
    }
}