# Cheetah.Core.Inbox.Mongo

MongoDB implementation of `IInboxStore` (`MongoInboxStore`) for idempotent consumption of incoming
events. The Mongo counterpart of `Cheetah.Core.Inbox.EntityFrameworkCore`.

## Dependencies

`Cheetah.Core`, `Cheetah.Core.Mongo`, `Cheetah.Core.Inbox`, `MongoDB.Driver`.
Module: `CrmInboxMongoModule` (`[DependsOn]` `CrmMongoModule`, `CrmInboxModule`).

## Behaviour

- **`AlreadyProcessedAsync`** checks for an existing `(EventId, ConsumerName)` record.
- **`AddAsync`** buffers the insert on the scoped `IMongoUnitOfWork`, so the idempotency record
  commits in the same transaction as the consumer's work.
- A **unique index on `(EventId, ConsumerName)`** is the backstop against duplicates and is created
  on initialization. `InboxMessage` has a composite key (no single `Id`), so MongoDB assigns an
  `ObjectId` `_id`; identity is enforced by the unique index, not `_id`.
- `DeleteOlderThanAsync` prunes by `ReceivedAt` in batches.

## Wiring

```csharp
[DependsOn(typeof(CrmInboxMongoModule))]
public partial class MyModule : CrmModule { }

services.Configure<MongoInboxStoreOptions>(o =>
{
    o.InboxCollection = "InboxMessages";
    o.ConnectionName = null;
});
```

The store is auto-registered via `[Export]`.

## Constraints

Requires a replica set (the buffered insert flushes through `IMongoUnitOfWork`'s transaction).
