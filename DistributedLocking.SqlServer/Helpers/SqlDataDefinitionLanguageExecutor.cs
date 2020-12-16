using System;
using System.Threading;
using System.Threading.Tasks;
using TheName.DistributedLocking.SqlServer.Abstractions.Helpers;
using static System.String;

namespace TheName.DistributedLocking.SqlServer.Helpers
{
    internal class SqlDataDefinitionLanguageExecutor : ISqlDataDefinitionLanguageExecutor
    {
        private const string DoesTableExistCommandFormat =
            "SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{0}' AND TABLE_NAME = '{1}';";
        
        private const string CreateSchemaAndTableIfNotExistsSqlCommandFormat = 
            "BEGIN TRANSACTION; " +
            "DECLARE @getapplock_result int; " +
            "EXEC @getapplock_result = sp_getapplock @Resource = '{0}', @LockMode = 'Exclusive', @LockTimeout = {2}; " +
            "IF @getapplock_result < 0 " +
            "BEGIN" +
            "   ROLLBACK TRANSACTION;" +
            "   SELECT @getapplock_result;" +
            "END " +
            "ELSE " +
            "BEGIN " +
            "  IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{0}')) " +
            "   BEGIN " +
            "      EXEC ('CREATE SCHEMA [{0}];') " +
            "   END " +
            "  IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{0}' AND TABLE_NAME = '{1}')) " +
            "   BEGIN" +
            "       {3} " +
            "       COMMIT TRANSACTION;  " +
            "       SELECT @getapplock_result;" +
            "   END " +
            "END; ";
        
        private readonly ISqlClient _sqlClient;

        public SqlDataDefinitionLanguageExecutor(ISqlClient sqlClient)
        {
            _sqlClient = sqlClient ?? throw new ArgumentNullException(nameof(sqlClient));
        }

        public async Task<bool> TableExistsAsync(string schemaName, string tableName, CancellationToken cancellationToken)
        {
            var commandText = GetDoesTableExistCommand(schemaName, tableName);
            var result = await _sqlClient.ExecuteScalarAsync<object>(commandText, cancellationToken).ConfigureAwait(false);
            return result != null;
        }

        public async Task CreateTableIfNotExistsAsync(
            string schemaName,
            string tableName,
            TimeSpan applicationLockTimeout,
            string createTableSqlCommandText,
            CancellationToken cancellationToken)
        {
            var commandText = GetCreateSchemaAndTableIfNotExistsCommand(
                schemaName,
                tableName,
                applicationLockTimeout,
                createTableSqlCommandText);
            
            var result = await _sqlClient.ExecuteScalarAsync<int>(commandText, cancellationToken).ConfigureAwait(false);
            ValidateGetAppLockResult(result, applicationLockTimeout);
        }

        private static string GetDoesTableExistCommand(string schemaName, string tableName) =>
            Format(DoesTableExistCommandFormat, schemaName, tableName);

        private static string GetCreateSchemaAndTableIfNotExistsCommand(
            string schemaName,
            string tableName,
            TimeSpan applicationLockTimeout,
            string createTableCommandText) =>
            Format(
                CreateSchemaAndTableIfNotExistsSqlCommandFormat,
                schemaName,
                tableName,
                (int) applicationLockTimeout.TotalMilliseconds,
                createTableCommandText);

        private static void ValidateGetAppLockResult(object result, TimeSpan applicationLockTimeout)
        {
            if (result is not int resultInt)
            {
                throw new InvalidOperationException("SQL script creating schema and table should have returned a result status code.");
            }

            switch ((GetAppLockResult)resultInt)
            {
                case GetAppLockResult.Successful:
                case GetAppLockResult.SuccessfulAfterWaiting:
                    return;
                
                case GetAppLockResult.Timeout:
                    throw new TimeoutException(
                        $"SQL script creating schema and table did not succeed because getting SQL application lock timeout out (timeout set to {(int) applicationLockTimeout.TotalMilliseconds} ms). Please try again");
                
                case GetAppLockResult.Cancelled:
                case GetAppLockResult.Deadlock:
                case GetAppLockResult.ParameterValidationOrOtherError:
                    throw new InvalidOperationException(
                        "SQL script creating schema and table failed to acquire application lock.");
                
                default:
                    throw new InvalidOperationException(
                        "Unexpected error during SQL script creating schema and table execution");
            }
        }
        
        private enum GetAppLockResult
        {
            Successful = 0,
            SuccessfulAfterWaiting = 1,
            Timeout = -1,
            Cancelled = -2,
            Deadlock = -3,
            ParameterValidationOrOtherError = -999
        }
    }
}