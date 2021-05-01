using Cassandra;
using Moq;

namespace DistributedLocking.Cassandra.UnitTests.Extensions
{
    internal static class PreparedStatementMockExtensions
    {
        public static Mock<PreparedStatement> SetupBinding(this Mock<PreparedStatement> preparedStatementMock, BoundStatement boundStatement)
        {
            preparedStatementMock
                .Setup(statement => statement.Bind(It.IsAny<object[]>()))
                .Returns(boundStatement);

            return preparedStatementMock;
        }
    }
}