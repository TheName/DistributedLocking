using System;
using System.Threading;
using System.Threading.Tasks;
using Cassandra;
using DistributedLocking.Abstractions;
using DistributedLocking.Cassandra.UnitTests.Extensions;
using DistributedLocking.Repositories.Cassandra;
using Moq;
using TestHelpers.Attributes;
using Xunit;

namespace DistributedLocking.Cassandra.UnitTests
{
    public class CassandraDistributedLockRepository_Should
    {
        private const string ExpectedPreparedInsertStatement = "UPDATE distributed_locks USING TTL :ttl SET id = :id WHERE resource_id = :resource_id IF id = NULL;";
        private const string ExpectedPreparedUpdateStatement = "UPDATE distributed_locks USING TTL :ttl SET id = :id WHERE resource_id = :resource_id IF id = :id;";
        private const string ExpectedPreparedDeleteStatement = "DELETE FROM distributed_locks WHERE resource_id = :resource_id IF id = :id;";
        
        [Fact]
        public void Throw_When_Creating_And_SessionIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CassandraDistributedLockRepository(null));
        }

        [Theory]
        [AutoMoqData]
        public void Throw_When_Creating_And_PrepareAsyncThrows(Mock<ISession> sessionMock)
        {
            sessionMock
                .Setup(session => session.PrepareAsync(It.IsAny<string>()))
                .Throws<Exception>();

            Assert.Throws<Exception>(() => new CassandraDistributedLockRepository(sessionMock.Object));
        }

        [Theory]
        [AutoMoqData]
        public void NotThrow_When_Creating_And_PrepareAsyncThrowsAsync(Mock<ISession> sessionMock, Exception exception)
        {
            sessionMock
                .Setup(session => session.PrepareAsync(It.IsAny<string>()))
                .ThrowsAsync(exception);

            _ = new CassandraDistributedLockRepository(sessionMock.Object);
        }

        [Theory]
        [AutoMoqData]
        public void StartPreparingInsertStatementAsync_When_Creating(Mock<ISession> sessionMock)
        {
            _ = new CassandraDistributedLockRepository(sessionMock.Object);

            sessionMock.Verify(session => session.PrepareAsync(ExpectedPreparedInsertStatement), Times.Once);
            sessionMock.Verify(
                session => session.PrepareAsync(It.Is<string>(s => s != ExpectedPreparedInsertStatement)),
                Times.Exactly(2));
            
            sessionMock.VerifyNoOtherCalls();
        }

        [Theory]
        [AutoMoqData]
        public void StartPreparingUpdateStatementAsync_When_Creating(Mock<ISession> sessionMock)
        {
            _ = new CassandraDistributedLockRepository(sessionMock.Object);

            sessionMock.Verify(session => session.PrepareAsync(ExpectedPreparedUpdateStatement), Times.Once);
            sessionMock.Verify(
                session => session.PrepareAsync(It.Is<string>(s => s != ExpectedPreparedUpdateStatement)),
                Times.Exactly(2));
            
            sessionMock.VerifyNoOtherCalls();
        }

        [Theory]
        [AutoMoqData]
        public void StartPreparingDeleteStatementAsync_When_Creating(Mock<ISession> sessionMock)
        {
            _ = new CassandraDistributedLockRepository(sessionMock.Object);

            sessionMock.Verify(session => session.PrepareAsync(ExpectedPreparedDeleteStatement), Times.Once);
            sessionMock.Verify(
                session => session.PrepareAsync(It.Is<string>(s => s != ExpectedPreparedDeleteStatement)),
                Times.Exactly(2));
            
            sessionMock.VerifyNoOtherCalls();
        }

        [Theory]
        [AutoMoqData]
        public async Task Throw_When_TryingToInsert_And_PreparedInsertStatementThrows(
            Mock<ISession> sessionMock,
            Exception exception,
            DistributedLockResourceId resourceId,
            DistributedLockId id,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken)
        {
            sessionMock
                .Setup(session => session.PrepareAsync(ExpectedPreparedInsertStatement))
                .ThrowsAsync(exception);

            var repository = new CassandraDistributedLockRepository(sessionMock.Object);

            var result = await Assert.ThrowsAsync<Exception>(() =>
                repository.TryInsert(
                    resourceId,
                    id,
                    timeToLive,
                    cancellationToken));

            Assert.Equal(exception, result);
        }

        [Theory]
        [AutoMoqData]
        public async Task Throw_When_TryingToInsert_And_ResourceIdIsNull(
            Mock<ISession> sessionMock,
            DistributedLockId id,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken)
        {
            var repository = new CassandraDistributedLockRepository(sessionMock.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                repository.TryInsert(
                    null,
                    id,
                    timeToLive,
                    cancellationToken));
        }

        [Theory]
        [AutoMoqData]
        public async Task Throw_When_TryingToInsert_And_IdIsNull(
            Mock<ISession> sessionMock,
            DistributedLockResourceId resourceId,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken)
        {
            var repository = new CassandraDistributedLockRepository(sessionMock.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                repository.TryInsert(
                    resourceId,
                    null,
                    timeToLive,
                    cancellationToken));
        }

        [Theory]
        [AutoMoqData]
        public async Task Throw_When_TryingToInsert_And_TimeToLiveIsNull(
            Mock<ISession> sessionMock,
            DistributedLockResourceId resourceId,
            DistributedLockId id,
            CancellationToken cancellationToken)
        {
            var repository = new CassandraDistributedLockRepository(sessionMock.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                repository.TryInsert(
                    resourceId,
                    id,
                    null,
                    cancellationToken));
        }

        [Theory]
        [AutoMoqData]
        public async Task BindParameters_When_TryingToInsert(
            Mock<ISession> sessionMock,
            Mock<PreparedStatement> preparedStatementMock,
            bool applied,
            DistributedLockResourceId resourceId,
            DistributedLockId id,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken)
        {
            sessionMock.SetupPreparedStatementWithRowSetResultWithSingleRowWithAppliedColumn(
                preparedStatementMock,
                applied);
            
            var repository = new CassandraDistributedLockRepository(sessionMock.Object);

            await repository.TryInsert(
                resourceId,
                id,
                timeToLive,
                cancellationToken);

            var verifyBoundObjects = new Func<object[], bool>(objects =>
            {
                dynamic singleObject = Assert.Single(objects);
                Assert.NotNull(singleObject);

                Assert.Equal(resourceId.ToString(), singleObject.resource_id);
                Assert.Equal((Guid) id, singleObject.id);
                Assert.Equal((int) ((TimeSpan) timeToLive).TotalSeconds, singleObject.ttl);
                return true;
            });

            preparedStatementMock
                .Verify(statement => statement.Bind(It.Is<object[]>(objects => verifyBoundObjects(objects))),
                    Times.Once);
        }

        [Theory]
        [AutoMoqData]
        public async Task SetConsistencyLevel_When_TryingToInsert(
            Mock<ISession> sessionMock,
            Mock<BoundStatement> boundStatementMock,
            bool applied,
            DistributedLockResourceId resourceId,
            DistributedLockId id,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken)
        {
            sessionMock.SetupPreparedStatementWithRowSetResultWithSingleRowWithAppliedColumn(
                boundStatementMock,
                applied);

            var repository = new CassandraDistributedLockRepository(sessionMock.Object);

            await repository.TryInsert(
                resourceId,
                id,
                timeToLive,
                cancellationToken);

            Assert.Equal(ConsistencyLevel.Quorum, boundStatementMock.Object.ConsistencyLevel);
        }

        [Theory]
        [AutoMoqData]
        public async Task SetSerialConsistencyLevel_When_TryingToInsert(
            Mock<ISession> sessionMock,
            Mock<BoundStatement> boundStatementMock,
            bool applied,
            DistributedLockResourceId resourceId,
            DistributedLockId id,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken)
        {
            sessionMock.SetupPreparedStatementWithRowSetResultWithSingleRowWithAppliedColumn(
                boundStatementMock,
                applied);

            var repository = new CassandraDistributedLockRepository(sessionMock.Object);

            await repository.TryInsert(
                resourceId,
                id,
                timeToLive,
                cancellationToken);

            Assert.Equal(ConsistencyLevel.Serial, boundStatementMock.Object.SerialConsistencyLevel);
        }

        [Theory]
        [AutoMoqWithInlineData(true)]
        [AutoMoqWithInlineData(false)]
        public async Task ReturnExpectedResult_When_TryingToInsert(
            bool expectedResult,
            Mock<ISession> sessionMock,
            DistributedLockResourceId resourceId,
            DistributedLockId id,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken)
        {
            sessionMock.SetupPreparedStatementWithRowSetResultWithSingleRowWithAppliedColumn(expectedResult);
            var repository = new CassandraDistributedLockRepository(sessionMock.Object);

            var result = await repository.TryInsert(
                resourceId,
                id,
                timeToLive,
                cancellationToken);

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [AutoMoqData]
        public async Task Throw_When_TryingToUpdate_And_PreparedUpdateStatementThrows(
            Mock<ISession> sessionMock,
            Exception exception,
            DistributedLockResourceId resourceId,
            DistributedLockId id,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken)
        {
            sessionMock
                .Setup(session => session.PrepareAsync(ExpectedPreparedUpdateStatement))
                .ThrowsAsync(exception);

            var repository = new CassandraDistributedLockRepository(sessionMock.Object);

            var result = await Assert.ThrowsAsync<Exception>(() =>
                repository.TryUpdateTimeToLiveAsync(
                    resourceId,
                    id,
                    timeToLive,
                    cancellationToken));

            Assert.Equal(exception, result);
        }

        [Theory]
        [AutoMoqData]
        public async Task Throw_When_TryingToUpdate_And_ResourceIdIsNull(
            Mock<ISession> sessionMock,
            DistributedLockId id,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken)
        {
            var repository = new CassandraDistributedLockRepository(sessionMock.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                repository.TryUpdateTimeToLiveAsync(
                    null,
                    id,
                    timeToLive,
                    cancellationToken));
        }

        [Theory]
        [AutoMoqData]
        public async Task Throw_When_TryingToUpdate_And_IdIsNull(
            Mock<ISession> sessionMock,
            DistributedLockResourceId resourceId,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken)
        {
            var repository = new CassandraDistributedLockRepository(sessionMock.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                repository.TryUpdateTimeToLiveAsync(
                    resourceId,
                    null,
                    timeToLive,
                    cancellationToken));
        }

        [Theory]
        [AutoMoqData]
        public async Task Throw_When_TryingToUpdate_And_TimeToLiveIsNull(
            Mock<ISession> sessionMock,
            DistributedLockResourceId resourceId,
            DistributedLockId id,
            CancellationToken cancellationToken)
        {
            var repository = new CassandraDistributedLockRepository(sessionMock.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                repository.TryUpdateTimeToLiveAsync(
                    resourceId,
                    id,
                    null,
                    cancellationToken));
        }

        [Theory]
        [AutoMoqData]
        public async Task BindParameters_When_TryingToUpdate(
            Mock<ISession> sessionMock,
            Mock<PreparedStatement> preparedStatementMock,
            bool applied,
            DistributedLockResourceId resourceId,
            DistributedLockId id,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken)
        {
            sessionMock.SetupPreparedStatementWithRowSetResultWithSingleRowWithAppliedColumn(
                preparedStatementMock,
                applied);
            
            var repository = new CassandraDistributedLockRepository(sessionMock.Object);

            await repository.TryUpdateTimeToLiveAsync(
                resourceId,
                id,
                timeToLive,
                cancellationToken);

            var verifyBoundObjects = new Func<object[], bool>(objects =>
            {
                dynamic singleObject = Assert.Single(objects);
                Assert.NotNull(singleObject);

                Assert.Equal(resourceId.ToString(), singleObject.resource_id);
                Assert.Equal((Guid) id, singleObject.id);
                Assert.Equal((int) ((TimeSpan) timeToLive).TotalSeconds, singleObject.ttl);
                return true;
            });

            preparedStatementMock
                .Verify(statement => statement.Bind(It.Is<object[]>(objects => verifyBoundObjects(objects))),
                    Times.Once);
        }

        [Theory]
        [AutoMoqData]
        public async Task SetConsistencyLevel_When_TryingToUpdate(
            Mock<ISession> sessionMock,
            Mock<BoundStatement> boundStatementMock,
            bool applied,
            DistributedLockResourceId resourceId,
            DistributedLockId id,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken)
        {
            sessionMock.SetupPreparedStatementWithRowSetResultWithSingleRowWithAppliedColumn(
                boundStatementMock,
                applied);

            var repository = new CassandraDistributedLockRepository(sessionMock.Object);

            await repository.TryUpdateTimeToLiveAsync(
                resourceId,
                id,
                timeToLive,
                cancellationToken);

            Assert.Equal(ConsistencyLevel.Quorum, boundStatementMock.Object.ConsistencyLevel);
        }

        [Theory]
        [AutoMoqData]
        public async Task SetSerialConsistencyLevel_When_TryingToUpdate(
            Mock<ISession> sessionMock,
            Mock<BoundStatement> boundStatementMock,
            bool applied,
            DistributedLockResourceId resourceId,
            DistributedLockId id,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken)
        {
            sessionMock.SetupPreparedStatementWithRowSetResultWithSingleRowWithAppliedColumn(
                boundStatementMock,
                applied);

            var repository = new CassandraDistributedLockRepository(sessionMock.Object);

            await repository.TryUpdateTimeToLiveAsync(
                resourceId,
                id,
                timeToLive,
                cancellationToken);

            Assert.Equal(ConsistencyLevel.Serial, boundStatementMock.Object.SerialConsistencyLevel);
        }

        [Theory]
        [AutoMoqWithInlineData(true)]
        [AutoMoqWithInlineData(false)]
        public async Task ReturnExpectedResult_When_TryingToUpdate(
            bool expectedResult,
            Mock<ISession> sessionMock,
            DistributedLockResourceId resourceId,
            DistributedLockId id,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken)
        {
            sessionMock.SetupPreparedStatementWithRowSetResultWithSingleRowWithAppliedColumn(expectedResult);
            var repository = new CassandraDistributedLockRepository(sessionMock.Object);

            var result = await repository.TryUpdateTimeToLiveAsync(
                resourceId,
                id,
                timeToLive,
                cancellationToken);

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [AutoMoqData]
        public async Task Throw_When_TryingToDelete_And_PreparedDeleteStatementThrows(
            Mock<ISession> sessionMock,
            Exception exception,
            DistributedLockResourceId resourceId,
            DistributedLockId id,
            CancellationToken cancellationToken)
        {
            sessionMock
                .Setup(session => session.PrepareAsync(ExpectedPreparedDeleteStatement))
                .ThrowsAsync(exception);

            var repository = new CassandraDistributedLockRepository(sessionMock.Object);

            var result = await Assert.ThrowsAsync<Exception>(() =>
                repository.TryDelete(
                    resourceId,
                    id,
                    cancellationToken));

            Assert.Equal(exception, result);
        }

        [Theory]
        [AutoMoqData]
        public async Task Throw_When_TryingToDelete_And_ResourceIdIsNull(
            Mock<ISession> sessionMock,
            DistributedLockId id,
            CancellationToken cancellationToken)
        {
            var repository = new CassandraDistributedLockRepository(sessionMock.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                repository.TryDelete(
                    null,
                    id,
                    cancellationToken));
        }

        [Theory]
        [AutoMoqData]
        public async Task Throw_When_TryingToDelete_And_IdIsNull(
            Mock<ISession> sessionMock,
            DistributedLockResourceId resourceId,
            CancellationToken cancellationToken)
        {
            var repository = new CassandraDistributedLockRepository(sessionMock.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                repository.TryDelete(
                    resourceId,
                    null,
                    cancellationToken));
        }

        [Theory]
        [AutoMoqData]
        public async Task BindParameters_When_TryingToDelete(
            Mock<ISession> sessionMock,
            Mock<PreparedStatement> preparedStatementMock,
            bool applied,
            DistributedLockResourceId resourceId,
            DistributedLockId id,
            CancellationToken cancellationToken)
        {
            sessionMock.SetupPreparedStatementWithRowSetResultWithSingleRowWithAppliedColumn(
                preparedStatementMock,
                applied);
            
            var repository = new CassandraDistributedLockRepository(sessionMock.Object);

            await repository.TryDelete(
                resourceId,
                id,
                cancellationToken);

            var verifyBoundObjects = new Func<object[], bool>(objects =>
            {
                dynamic singleObject = Assert.Single(objects);
                Assert.NotNull(singleObject);

                Assert.Equal(resourceId.ToString(), singleObject.resource_id);
                Assert.Equal((Guid) id, singleObject.id);
                return true;
            });

            preparedStatementMock
                .Verify(statement => statement.Bind(It.Is<object[]>(objects => verifyBoundObjects(objects))),
                    Times.Once);
        }

        [Theory]
        [AutoMoqData]
        public async Task SetConsistencyLevel_When_TryingToDelete(
            Mock<ISession> sessionMock,
            Mock<BoundStatement> boundStatementMock,
            bool applied,
            DistributedLockResourceId resourceId,
            DistributedLockId id,
            CancellationToken cancellationToken)
        {
            sessionMock.SetupPreparedStatementWithRowSetResultWithSingleRowWithAppliedColumn(
                boundStatementMock,
                applied);

            var repository = new CassandraDistributedLockRepository(sessionMock.Object);

            await repository.TryDelete(
                resourceId,
                id,
                cancellationToken);

            Assert.Equal(ConsistencyLevel.Quorum, boundStatementMock.Object.ConsistencyLevel);
        }

        [Theory]
        [AutoMoqData]
        public async Task SetSerialConsistencyLevel_When_TryingToDelete(
            Mock<ISession> sessionMock,
            Mock<BoundStatement> boundStatementMock,
            bool applied,
            DistributedLockResourceId resourceId,
            DistributedLockId id,
            CancellationToken cancellationToken)
        {
            sessionMock.SetupPreparedStatementWithRowSetResultWithSingleRowWithAppliedColumn(
                boundStatementMock,
                applied);

            var repository = new CassandraDistributedLockRepository(sessionMock.Object);

            await repository.TryDelete(
                resourceId,
                id,
                cancellationToken);

            Assert.Equal(ConsistencyLevel.Serial, boundStatementMock.Object.SerialConsistencyLevel);
        }

        [Theory]
        [AutoMoqWithInlineData(true)]
        [AutoMoqWithInlineData(false)]
        public async Task ReturnExpectedResult_When_TryingToDelete(
            bool expectedResult,
            Mock<ISession> sessionMock,
            DistributedLockResourceId resourceId,
            DistributedLockId id,
            CancellationToken cancellationToken)
        {
            sessionMock.SetupPreparedStatementWithRowSetResultWithSingleRowWithAppliedColumn(expectedResult);
            var repository = new CassandraDistributedLockRepository(sessionMock.Object);

            var result = await repository.TryDelete(
                resourceId,
                id,
                cancellationToken);

            Assert.Equal(expectedResult, result);
        }
    }
}