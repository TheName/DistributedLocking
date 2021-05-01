CREATE TABLE IF NOT EXISTS distributed_locks (
    resource_id     text    PRIMARY KEY,
    id              uuid
) WITH  gc_grace_seconds = 0
    AND ID='{{TABLE_ID}}';