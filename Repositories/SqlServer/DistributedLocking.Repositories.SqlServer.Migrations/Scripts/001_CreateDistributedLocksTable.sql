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
    END