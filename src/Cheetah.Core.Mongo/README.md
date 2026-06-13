# Cheetah.Core.Mongo

MongoDB data-access layer for Cheetah. Implements the shared repository contracts
(`IRepository<,>` / `IReadOnlyRepository<,>` from `Cheetah.Core.DataAccess`) on top of the official
`MongoDB.Driver`, so the application layer (CQRS handlers) is identical whether an entity is backed
by EF Core, Dapper or MongoDB.

It is designed to **coexist** with the EF/Dapper stacks: bind only the entities you want served from
Mongo. There is **no provider sub-package** — MongoDB ships a single official driver (unlike the
Dapper SQL stack with its per-dialect packages).

## Dependencies

- `Cheetah.Core`, `Cheetah.Core.Domain`, `Cheetah.Core.Specification`, `Cheetah.Core.DataAccess`
- `MongoDB.Driver`

Module: `CrmMongoModule` (`[DependsOn]` Core, Domain, Specification, DataAccess).

## How it works

| Concern | Type | Notes |
|---|---|---|
| Client cache | `IMongoClientProvider` | One `IMongoClient` per connection string (singleton, pooled). |
| Database resolve | `IMongoDatabaseProvider` | Resolves the connection string via `IConnectionStringResolver`; database name is taken from the URI. |
| Collection resolve | `IMongoCollectionResolver` | Entity → collection (from the map) + database (from `[ConnectionStringName]`). |
| Mapping | `MongoEntityMap<T>`, conventions | Fluent map (collection/key/element rename/ignore) or convention fallback. |
| Specifications | `IMongoFilterBuilder` | Renders `ISpecification<T>.ToExpression()` natively to a `FilterDefinition<T>`; native filters via `MongoSpecification<T>`. |
| Write path | `IMongoUnitOfWork` | Buffers Add/Update/Delete, flushes in a session transaction on `SaveChangesAsync`. |
| Repository | `MongoRepository<TEntity,TKey>` | The `IRepository<,>` implementation. |
| Raw reads | `IMongoQueryExecutor` | Projections/aggregations for grids/reports (no transaction). |

### Mapping conventions (important)

`MongoMappingConventions.EnsureRegistered()` runs in `CrmMongoModule.ConfigureServices` and installs
process-wide conventions:

- **`DomainEvents` is unmapped** on every aggregate — the Mongo mirror of EF's
  `builder.Ignore(e => e.DomainEvents)`. Without it, domain events would be persisted.
- **Non-public-setter properties are mapped.** Cheetah entities use `private`/`protected set` by
  design; the driver's default member finder maps only public read-write properties, which would
  persist *nothing*. The `MapWritablePropertiesConvention` maps every property with a public getter
  and any setter. Read-only computed properties (no setter) are skipped.
- **`Entity<TId>.Id` → `_id`.** The inherited key (non-public setter) is designated as `_id`.
- `Guid` is stored as `GuidRepresentation.Standard`; `DateTimeOffset` as a BSON UTC date (so range
  queries work).

## Usage

```csharp
// 1) Depend on the module
[DependsOn(typeof(CrmMongoModule))]
public partial class MyDataAccessModule : CrmModule { }

// 2) (Optional) explicit map — otherwise conventions apply
[Export(LifetimeType.Singleton, typeof(IMongoEntityMap))]
public sealed class CustomerMap : MongoEntityMap<Customer>
{
    public CustomerMap()
    {
        ToCollection("customers");
        HasKey(x => x.Id);
        Field(x => x.FullName, "full_name");
        Ignore(x => x.TemporaryFlag);
    }
}

// 3) Bind the repository per entity (opt-in — does not replace EF/Dapper globally)
services.AddMongoRepository<Customer>();

// 4) Inject IRepository<Customer> in handlers exactly as with EF/Dapper.
```

Connection string (database name **must** be present in the URI):

```json
{
  "ConnectionStrings": {
    "Default": "mongodb://localhost:27017/cheetah"
  }
}
```

Select a non-default database per entity with `[ConnectionStringName("Reporting")]`.

## Constraints

- **Multi-document transactions require a replica set** (or a sharded cluster). `SaveChangesAsync`
  opens a session transaction; on a standalone `mongod` it will fail. For local/dev, run a
  single-node replica set.
- Atomicity spans one connection: writes to different `[ConnectionStringName]` values commit in
  separate transactions (same semantics as the Dapper unit of work).
- `AsQueryable()` is genuinely supported here (the driver's LINQ provider), unlike the Dapper
  repository which throws.
- All filtering must go through specifications (project rule); ad-hoc reads use `IMongoQueryExecutor`.
