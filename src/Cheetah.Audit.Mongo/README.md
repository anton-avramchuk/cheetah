# Cheetah.Audit.Mongo

MongoDB implementation of the audit storage contracts: `IAuditSink` (`MongoAuditSink`) and
`IAuditPublishStore` (`MongoAuditPublishStore`). The Mongo counterpart of
`Cheetah.Audit.EntityFrameworkCore`.

## Dependencies

`Cheetah.Core`, `Cheetah.Core.Mongo`, `Cheetah.Audit`, `MongoDB.Driver`.
Module: `CrmAuditMongoModule` (`[DependsOn]` `CrmMongoModule`, `CrmAuditModule`).

## Behaviour

- **`MongoAuditSink.EmitAsync`** buffers an `InsertMany` on the scoped `IMongoUnitOfWork`, so audit
  entries commit in the same transaction as the audited aggregate (the audit interceptor calls the
  sink before the unit of work flushes), mirroring the EF sink.
- **`MongoAuditPublishStore`** drives the Kafka publisher: `ClaimPendingAsync` reads a batch of
  unpublished entries (`PublishedAt == null`, `NextAttemptAt <= now`, ordered by `OccurredAt`) and
  pushes `NextAttemptAt` forward so another replica skips them for the claim window;
  `MarkPublishedAsync` / `MarkFailedAsync` record the outcome.
  > As with the EF base store, the claim is race-prone without `SELECT … FOR UPDATE SKIP LOCKED`
  > semantics; the claim-timeout window keeps double-publishing rare and idempotent consumers handle
  > the rest.

## Wiring

```csharp
[DependsOn(typeof(CrmAuditMongoModule))]
public partial class MyModule : CrmModule { }

services.Configure<MongoAuditStoreOptions>(o =>
{
    o.AuditCollection = "AuditEntries";
    o.ConnectionName = null;
});
```

Both services are auto-registered via `[Export]`. The module creates the
`(PublishedAt, NextAttemptAt, OccurredAt)` index on initialization.

## Constraints

Requires a replica set (the buffered insert flushes through `IMongoUnitOfWork`'s transaction).
