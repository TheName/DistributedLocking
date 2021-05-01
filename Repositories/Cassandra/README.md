# DistributedLocking.Repositories.Cassandra

Cassandra repository implementation for DistributedLocking

## Migrations

You can find migrations provider [here](./DistributedLocking.Repositories.Cassandra.Migrations/CassandraDistributedLocksRepositoryMigrationsProvider.cs) and if you want to run required scripts manually, go [here](./DistributedLocking.Repositories.Cassandra.Migrations/Scripts).

## Configuration

In order to properly use this package, you need to register Cassandra's *ISession* on your own.

## Dependency Injection

You can find a dependency injection package [here](./Extensions/Extensions.Cassandra.DependencyInjection) and use it like

```csharp
services.AddCassandraDistributedLocking();
```

## Sample application

You can find sample application [here](./Samples/DistributedLocking.Repositories.Cassandra.Samples/Program.cs)