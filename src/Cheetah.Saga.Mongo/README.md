# Cheetah.Saga.Mongo

MongoDB implementation of `ISagaRepository` (`MongoSagaRepository`) with optimistic concurrency. The
Mongo counterpart of `Cheetah.Saga.EntityFrameworkCore`.

## Dependencies

`Cheetah.Core`, `Cheetah.Core.Mongo`, `Cheetah.Saga`, `MongoDB.Driver`.
Module: `CrmSagaMongoModule` (`[DependsOn]` `CrmMongoModule`, `CrmSagaModule`).

## Behaviour

MongoDB has no change tracker, so the scoped repository tracks instances itself:

- **`FindAsync`** loads an instance and records its current `Version`.
- **`AddAsync`** marks an instance as new.
- **`SaveChangesAsync`** persists all tracked instances in a session transaction: new ones are
  inserted; existing ones are replaced with a `Version` guard (`Id == id && Version == original`).
  A replace that matches **no** document means a concurrent update won → `SagaConcurrencyException`
  (same contract as the EF repository's `DbUpdateConcurrencyException` mapping). On success `Version`
  is incremented and `UpdatedAt` refreshed.
- A **unique index on `(SagaType, CorrelationKey)`** is created on initialization.

## Wiring

```csharp
[DependsOn(typeof(CrmSagaMongoModule))]
public partial class MyModule : CrmModule { }

services.Configure<MongoSagaStoreOptions>(o =>
{
    o.SagaCollection = "SagaInstances";
    o.ConnectionName = null;
});
```

The repository is auto-registered via `[Export]`.

## Constraints

Requires a replica set (multi-document transactions).
