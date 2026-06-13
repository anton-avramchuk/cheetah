# Cheetah.Core.Outbox.Mongo

MongoDB implementation of the Outbox pattern stores: `IOutboxStore` (`MongoOutboxStore`) and
`IDeadLetterStore` (`MongoDeadLetterStore`). The Mongo counterpart of
`Cheetah.Core.Outbox.EntityFrameworkCore`.

## Dependencies

`Cheetah.Core`, `Cheetah.Core.Mongo`, `Cheetah.Core.Outbox`, `MongoDB.Driver`.
Module: `CrmOutboxMongoModule` (`[DependsOn]` `CrmMongoModule`, `CrmOutboxModule`).

## Behaviour

- **`AddAsync`** buffers the insert on the scoped `IMongoUnitOfWork`, so the outbox message commits
  in the **same transaction** as the aggregate (mirrors the EF store's deferred `Add` + shared
  `SaveChanges`). Publish the events after `SaveChangesAsync`, as everywhere in Cheetah.
- Processor methods (`GetPendingAsync`, `MarkProcessed[Batch]Async`, `MarkFailedAsync`,
  `DeleteProcessedAsync`) read/update the collection directly.
- `MongoDeadLetterStore.MoveFromOutboxAsync` / `RequeueAsync` run their insert+delete pair inside a
  session transaction (atomic).

## Wiring

```csharp
[DependsOn(typeof(CrmOutboxMongoModule))]
public partial class MyModule : CrmModule { }

// Optional: collection / connection names
services.Configure<MongoOutboxStoreOptions>(o =>
{
    o.OutboxCollection = "OutboxMessages";
    o.DeadLetterCollection = "DeadLetterMessages";
    o.ConnectionName = null; // default connection
});
```

Both stores are auto-registered via `[Export]`. The module creates the
`(ProcessedAt, NextAttemptAt, OccurredAt)` index on initialization.

## Constraints

Requires a replica set (multi-document transactions). `ConnectionName` should match the connection
the aggregate writes to, so the outbox insert shares its transaction.
