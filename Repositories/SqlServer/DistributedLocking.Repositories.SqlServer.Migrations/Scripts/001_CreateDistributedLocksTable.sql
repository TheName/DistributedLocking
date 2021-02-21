BEGIN TRANSACTION;
DECLARE @getapplock_result int;
EXEC @getapplock_result = sp_getapplock @Resource = 'DistributedLocking', @LockMode = 'Exclusive', @LockTimeout = 3000;
IF @getapplock_result < 0
    BEGIN 
        ROLLBACK TRANSACTION;
        ; THROW 50000, 'Could not acquire application lock within 3000 ms timeout', 1 ;
    END
ELSE 
    BEGIN
        IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = 'DistributedLocking'))
            BEGIN
                EXEC ('CREATE SCHEMA [DistributedLocking];')
            END
        IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'DistributedLocking' AND TABLE_NAME = 'DistributedLocks'))
            BEGIN
                CREATE TABLE [DistributedLocking].DistributedLocks (
                   ResourceId              VARCHAR(900)         NOT NULL PRIMARY KEY,
                   LockId                  UNIQUEIDENTIFIER     NOT NULL UNIQUE, 
                   ExpiryDateTimestamp     DATETIME2            NOT NULL); 
                CREATE INDEX IDX_ResourceId_ExpiryDateTimestamp ON [DistributedLocking].DistributedLocks (ResourceId, ExpiryDateTimestamp);
                CREATE INDEX IDX_ResourceId_LockId_ExpiryDateTimestamp ON [DistributedLocking].DistributedLocks (ResourceId, LockId, ExpiryDateTimestamp);
                COMMIT TRANSACTION;
            END
    END;