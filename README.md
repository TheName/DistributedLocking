# Distributed Locking

Simple C# implementation of distributed lock that can be used in multi-threaded environment focused on extensibility.

## Quickstart

The library is designed to be used via abstractions, so regardless of the underlying repository you choose, the basic operations remain the same.

Below you can find the basic usage examples.

### Acquire the lock

In order to acquire a lock, you need to provide:
- an *identifier* - something, that identifies your resource,
- desired TTL (time to live) of the lock - in case your process would not gracefully release the lock, it has to automatically turn itself inactive to avoid infinite locks.

To do that, you have a simple interface:
```csharp
IDistributedLockFacade facade = ...
var identifier = new DistributedLockIdentifier("Resource ID");
var ttl = new DistributedLockTimeToLive(TimeSpan.FromMinutes(5);
var (success, distributedLock) = await facade.TryAcquireAsync(identifier, ttl, CancellationToken.None);
```

If `success` is `False`, the `distributedLock` will be `null` - the lock was not acquired; most likely the resource is already locked.

If `success` is `True`, the `distributedLock` will be not null - the lock was successfully acquired.

### Extend the lock

In case your work requires more time than the previous TTL you've provided, simply extend it:

```csharp
var ttl = new DistributedLockTimeToLive(TimeSpan.FromMinutes(5);
var success = await distributedLock.TryExtendAsync(ttl, CancellationToken.None);
```

If `success` is `False` - extending the lock failed; most likely it's already inactive. You should acquire a new lock to continue the work.

If `success` is `True` - extending the lock succeed. Your lock is active for the provided TTL value (5 minutes in this case).

### Release the lock

The simplest way to ensure the lock is released once you're done with your work is to use it's `IAsyncDisposable` implementation;

```csharp
await using(distributedLock)
{
    // do your work.
}
```

## Repositories

This library is designed to work with any kind of repository as long as it implements required interface.

Currently supported databases:

- [SqlServer](./Repositories/SqlServer/README.MD)

### Migrations

Remember to run migrations before using the code.

Migration scripts are exposed by `IDistributedLocksRepositoryMigrationsProvider` and you can find packages with extensions to other known database migrations management packages;

- [DbUp extensions](./Repositories/Migrations/Repositories.Migrations.DbUp)