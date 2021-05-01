using Cassandra;
using Moq;

namespace DistributedLocking.Cassandra.UnitTests.Extensions
{
    internal static class SessionMockExtensions
    {
        public static Mock<ISession> SetupPreparedStatementWithRowSetResultWithSingleRowWithAppliedColumn(this Mock<ISession> sessionMock, bool appliedResult) =>
            sessionMock.SetupPreparedStatementWithRowSetResultWithSingleRowWithAppliedColumn(
                new Mock<BoundStatement>(),
                appliedResult);

        public static Mock<ISession> SetupPreparedStatementWithRowSetResultWithSingleRowWithAppliedColumn(
            this Mock<ISession> sessionMock,
            Mock<PreparedStatement> preparedStatementMock,
            bool appliedResult) =>
            sessionMock.SetupPreparedStatementWithRowSetResultWithSingleRowWithAppliedColumn(
                preparedStatementMock,
                new Mock<BoundStatement>(),
                appliedResult);

        public static Mock<ISession> SetupPreparedStatementWithRowSetResultWithSingleRowWithAppliedColumn(
            this Mock<ISession> sessionMock,
            Mock<BoundStatement> boundStatementMock,
            bool appliedResult) =>
            sessionMock.SetupPreparedStatementWithRowSetResultWithSingleRowWithAppliedColumn(
                new Mock<PreparedStatement>(),
                boundStatementMock,
                appliedResult);

        private static Mock<ISession> SetupPreparedStatementWithRowSetResultWithSingleRowWithAppliedColumn(
            this Mock<ISession> sessionMock,
            Mock<PreparedStatement> preparedStatementMock,
            Mock<BoundStatement> boundStatementMock,
            bool appliedResult)
        {
            var rowSetMock = new Mock<RowSet>();
            rowSetMock.SetupEnumeratorWithSingleRowAndAppliedColumnValue(appliedResult);
            preparedStatementMock.SetupBinding(boundStatementMock.Object);
            sessionMock
                .SetupPreparedStatement(preparedStatementMock.Object)
                .SetupRowSetResult(boundStatementMock.Object, rowSetMock.Object);

            return sessionMock;
        }
        
        public static Mock<ISession> SetupPreparedStatement(this Mock<ISession> sessionMock, PreparedStatement preparedStatement)
        {
            sessionMock
                .Setup(session => session.PrepareAsync(It.IsAny<string>()))
                .ReturnsAsync(preparedStatement);

            return sessionMock;
        }
        
        public static Mock<ISession> SetupRowSetResult(this Mock<ISession> sessionMock, BoundStatement boundStatement, RowSet rowSet)
        {
            sessionMock
                .Setup(session => session.ExecuteAsync(boundStatement))
                .ReturnsAsync(rowSet);

            return sessionMock;
        }
    }
}