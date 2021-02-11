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
    /// <inheritdoc />
    public class SqlServerDistributedLocksRepository : IDistributedLocksRepository
    {
        private const string InsertIfNotExistsSqlCommand = 
           @"SET NOCOUNT ON;
                DELETE
                    FROM [DistributedLocking].[DistributedLocks] 
                    WHERE   Identifier = @Identifier
                    AND     ExpiryDateTimestamp < SYSUTCDATETIME();
            SET NOCOUNT OFF;
            INSERT INTO [DistributedLocking].[DistributedLocks] 
                SELECT 
                    @Identifier,
                    @Id,
                    DATEADD(millisecond,@ExpiryDateTimeSpanInMilliseconds,SYSUTCDATETIME()) 
                WHERE
                    NOT EXISTS 
                        (SELECT *
                            FROM [DistributedLocking].[DistributedLocks] WITH (UPDLOCK, HOLDLOCK)
                            WHERE       Identifier = @Identifier
                                AND     ExpiryDateTimestamp > SYSUTCDATETIME());";

        private const string UpdateDistributedLockIfExistsSqlCommand =
            @"UPDATE [DistributedLocking].[DistributedLocks] 
                SET
                    ExpiryDateTimestamp = DATEADD(millisecond,@ExpiryDateTimeSpanInMilliseconds,SYSUTCDATETIME()) 
                WHERE   Identifier = @Identifier
                AND     Id = @Id 
                AND     ExpiryDateTimestamp > SYSUTCDATETIME();";

        private const string DeleteDistributedLockIfExistsSqlCommand =
            @"DELETE
                    FROM [DistributedLocking].[DistributedLocks] 
                    WHERE   Identifier = @Identifier
                    AND     Id = @Id 
                    AND     ExpiryDateTimestamp > SYSUTCDATETIME();";

        private readonly ISqlClient _sqlClient;

        internal SqlServerDistributedLocksRepository(ISqlClient sqlClient)
        {
            _sqlClient = sqlClient ?? throw new ArgumentNullException(nameof(sqlClient));
        }

        /// <inheritdoc />
        public async Task<bool> TryInsert(
            DistributedLockIdentifier identifier,
            DistributedLockId id,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken)
        {
            var numberOfAffectedRows = await _sqlClient.ExecuteNonQueryAsync(
                    InsertIfNotExistsSqlCommand,
                    new List<SqlParameter>
                    {
                        GetIdentifierParameter(identifier),
                        GetIdParameter(id),
                        GetExpiryDateTimeSpanInMillisecondsParameter(timeToLive)
                    },
                    cancellationToken)
                .ConfigureAwait(false);

            return ParseNumberOfAffectedRowsToResult(numberOfAffectedRows);
        }

        /// <inheritdoc />
        public async Task<bool> TryUpdateTimeToLiveAsync(
            DistributedLockIdentifier identifier,
            DistributedLockId id,
            DistributedLockTimeToLive timeToLive,
            CancellationToken cancellationToken)
        {
            var numberOfAffectedRows = await _sqlClient.ExecuteNonQueryAsync(
                    UpdateDistributedLockIfExistsSqlCommand,
                    new[] 
                    {
                        GetIdentifierParameter(identifier),
                        GetIdParameter(id),
                        GetExpiryDateTimeSpanInMillisecondsParameter(timeToLive)
                    },
                    cancellationToken)
                .ConfigureAwait(false);

            return ParseNumberOfAffectedRowsToResult(numberOfAffectedRows);
        }

        /// <inheritdoc />
        public async Task<bool> TryDelete(
            DistributedLockIdentifier identifier,
            DistributedLockId id,
            CancellationToken cancellationToken)
        {
            var numberOfAffectedRows = await _sqlClient.ExecuteNonQueryAsync(
                    DeleteDistributedLockIfExistsSqlCommand,
                    new[]
                    {
                        GetIdentifierParameter(identifier),
                        GetIdParameter(id)
                    },
                    cancellationToken)
                .ConfigureAwait(false);

            return ParseNumberOfAffectedRowsToResult(numberOfAffectedRows);
        }

        private static SqlParameter GetIdentifierParameter(string identifier) =>
            new SqlParameter("@Identifier", SqlDbType.VarChar)
            {
                Value = identifier
            };

        private static SqlParameter GetIdParameter(Guid id) =>
            new SqlParameter("@Id", SqlDbType.UniqueIdentifier)
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