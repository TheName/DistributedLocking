# Distributed Locking 

[![Unit tests](https://github.com/TheName/DistributedLocking/actions/workflows/unit_tests.yml/badge.svg)](https://github.com/TheName/DistributedLocking/actions/workflows/unit_tests.yml)

[![SQL Server](https://github.com/TheName/DistributedLocking/actions/workflows/integration_tests_sqlserver.yml/badge.svg)](https://github.com/TheName/DistributedLocking/actions/workflows/integration_tests_sqlserver.yml)
[![Cassandra](https://github.com/TheName/DistributedLocking/actions/workflows/integration_tests_cassandra.yml/badge.svg)](https://github.com/TheName/DistributedLocking/actions/workflows/integration_tests_cassandra.yml)

[![Publish packages to NuGet](https://github.com/TheName/DistributedLocking/actions/workflows/publish_nuget.yml/badge.svg)](https://github.com/TheName/DistributedLocking/actions/workflows/publish_nuget.yml)

Simple C# implementation of distributed lock that can be used in multi-client (process) environment focused on extensibility.

## Quickstart

The library is designed to be used via abstractions, so regardless of the underlying repository you choose, the basic operations remain the same.

Below you can find the basic usage examples.

### Acquire the lock

In order to acquire a lock, you need to provide:
- a *resource id* - something, that identifies your resource,
- desired TTL (time to live) of the lock - in case your process would not gracefully release the lock, it has to automatically turn itself inactive to avoid infinite locks.

To do that, you have a simple interface:
```csharp
// IDistributedLockProvider provider = ... 
var resourceId = "Resource ID";
var ttl = TimeSpan.FromMinutes(5);
var distributedLock = await provider.TryAcquireAsync(
    resourceId,
    ttl,
    CancellationToken.None);
```

If acquiring fails, the `distributedLock` variable will be null.

### Extend the lock

In case your work requires more time than the previous TTL you've provided, simply extend it:

**NOTE:** The provided TTL overwrites previously assigned TTL.

```csharp
var ttl = TimeSpan.FromMinutes(5);
var success = await distributedLock.TryExtendAsync(ttl, CancellationToken.None);
```

### Release the lock

The simplest way to ensure the lock is released once you're done with your work is to use it's `IAsyncDisposable` implementation;

```csharp
await using(distributedLock)
{
    // do your work.
}
```

You can also release the lock explicitly:

```csharp
var success = await distributedLock.TryReleaseAsync(CancellationToken.None);
```

## Repositories

This library is designed to work with any kind of repository as long as it implements required interface.

Currently supported databases:

- [SqlServer](./Repositories/SqlServer/README.MD)
- [Cassandra](./Repositories/Cassandra/README.md)

### Migrations

Remember to run migrations before using the code.

Migration scripts are exposed by `IDistributedLocksRepositoryMigrationsProvider` and you can find packages with extensions to other known database migrations management packages;

- [DbUp extensions](./Repositories/Migrations/Repositories.Migrations.DbUp)
