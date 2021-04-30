using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using DistributedLocking.Abstractions;
using DistributedLocking.Abstractions.Repositories;
using DistributedLocking.Repositories.SqlServer.Abstractions.Helpers;

namespace DistributedLocking.Repositories.SqlServer
{
    internal class SqlServerDistributedLocksRepository : IDistributedLocksRepository
    {
        private const string InsertIfNotExistsSqlCommand = 
           @"DECLARE @CurrentTimestamp datetime2 = SYSUTCDATETIME();
            SET NOCOUNT ON;
                DELETE
                    FROM [DistributedLocking].[DistributedLocks] 
                    WHERE   ResourceId = @ResourceId
                    AND     ExpiryDateTimestamp < @CurrentTimestamp;
            SET NOCOUNT OFF;
            INSERT INTO [DistributedLocking].[DistributedLocks] 
                SELECT 
                    @ResourceId,
                    @LockId,
                    DATEADD(millisecond,@ExpiryDateTimeSpanInMilliseconds,@CurrentTimestamp) 
                WHERE
                    NOT EXISTS 
                        (SELECT *
                            FROM [DistributedLocking].[DistributedLocks] WITH (UPDLOCK, HOLDLOCK)
                            WHERE       ResourceId = @ResourceId
                                AND     ExpiryDateTimestamp > @CurrentTimestamp);";

        private const string UpdateDistributedLockIfExistsSqlCommand =
            @"DECLARE @CurrentTimestamp datetime2 = SYSUTCDATETIME();
            UPDATE [DistributedLocking].[DistributedLocks] 
                SET
                    ExpiryDateTimestamp = DATEADD(millisecond,@ExpiryDateTimeSpanInMilliseconds,@CurrentTimestamp) 
                WHERE   ResourceId = @ResourceId
                AND     LockId = @LockId 
                AND     ExpiryDateTimestamp > @CurrentTimestamp;";

        private const string DeleteDistributedLockIfExistsSqlCommand =
            @"DECLARE @CurrentTimestamp datetime2 = SYSUTCDATETIME();
            DELETE
                    FROM [DistributedLocking].[DistributedLocks] 
                    WHERE   ResourceId = @ResourceId
                    AND     LockId = @LockId 
                    AND     ExpiryDateTimestamp > @CurrentTimestamp;";

        private readonly ISqlClient _sqlClient;

        internal SqlServerDistributedLocksRepository(ISqlClient sqlClient)
        {
            _sqlClient = sqlClient ?? throw new ArgumentNullException(nameof(sqlClient));
        }

        public async Task<bool> TryInsert(
            DistributedLockResourceId resourceId,
            DistributedLockId id,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken)
        {
            var numberOfAffectedRows = await _sqlClient.ExecuteNonQueryAsync(
                    InsertIfNotExistsSqlCommand,
                    new List<SqlParameter>
                    {
                        GetResourceIdParameter(resourceId),
                        GetLockIdParameter(id),
                        GetExpiryDateTimeSpanInMillisecondsParameter(timeToLive)
                    },
                    cancellationToken)
                .ConfigureAwait(false);

            return ParseNumberOfAffectedRowsToResult(numberOfAffectedRows);
        }

        public async Task<bool> TryUpdateTimeToLiveAsync(
            DistributedLockResourceId resourceId,
            DistributedLockId id,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken)
        {
            var numberOfAffectedRows = await _sqlClient.ExecuteNonQueryAsync(
                    UpdateDistributedLockIfExistsSqlCommand,
                    new[] 
                    {
                        GetResourceIdParameter(resourceId),
                        GetLockIdParameter(id),
                        GetExpiryDateTimeSpanInMillisecondsParameter(timeToLive)
                    },
                    cancellationToken)
                .ConfigureAwait(false);

            return ParseNumberOfAffectedRowsToResult(numberOfAffectedRows);
        }

        public async Task<bool> TryDelete(
            DistributedLockResourceId resourceId,
            DistributedLockId id,
            CancellationToken cancellationToken)
        {
            var numberOfAffectedRows = await _sqlClient.ExecuteNonQueryAsync(
                    DeleteDistributedLockIfExistsSqlCommand,
                    new[]
                    {
                        GetResourceIdParameter(resourceId),
                        GetLockIdParameter(id)
                    },
                    cancellationToken)
                .ConfigureAwait(false);

            return ParseNumberOfAffectedRowsToResult(numberOfAffectedRows);
        }

        private static SqlParameter GetResourceIdParameter(string resourceId) =>
            new SqlParameter("@ResourceId", SqlDbType.VarChar)
            {
                Value = resourceId
            };

        private static SqlParameter GetLockIdParameter(Guid id) =>
            new SqlParameter("@LockId", SqlDbType.UniqueIdentifier)
            {
                Value = id
            };

        private static SqlParameter GetExpiryDateTimeSpanInMillisecondsParameter(TimeSpan timeSpan) =>
            new SqlParameter("@ExpiryDateTimeSpanInMilliseconds", SqlDbType.BigInt)
            {
                Value = timeSpan.TotalMilliseconds
            };

        private static bool ParseNumberOfAffectedRowsToResult(int numberOfAffectedRows) =>
            numberOfAffectedRows switch
            {
                0 => false,
                1 => true,
                _ => throw new InvalidOperationException(
                    $"Distributed lock statement should have affected 1 or 0 rows, but it affected {numberOfAffectedRows}")
            };
    }
}