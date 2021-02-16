# DistributedLocking.Repositories.SqlServer 

SQL Server repository implementation for DistributedLocking

## Migrations

You can find migrations provider [here](./DistributedLocking.Repositories.SqlServer.Migrations/SqlDistributedLocksRepositoryMigrationsProvider.cs) and if you want to run required scripts manually, go [here](./DistributedLocking.Repositories.SqlServer.Migrations/Scripts).

There is also [DbUp package prepared](./Extensions/Extensions.SqlServer.Migrations.DbUp).

## Configuration

Required configuration is very small and consists only of the connection string;

```json
{
  "SqlServerDistributedLockConfiguration": {
    "ConnectionString": "Server=localhost;Database=master;Trusted_Connection=True;"
  }
}
```

You can provide the configuration via code to override the default registration, if you prefer.

```csharp
var configuration = ...

services
    .AddSqlServerDistributedLocking()
    .AddSingleton<ISqlServerDistributedLockConfiguration>(configuration);
```

## Dependency Injection

You can find a dependency injection package [here](./Extensions/Extensions.SqlServer.DependencyInjection) and use it like

```csharp
services.AddSqlServerDistributedLocking();
```

## Sample application

You can find sample application [here](./Samples/DistributedLocking.Repositories.SqlServer.Samples/Program.cs)