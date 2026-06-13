# Cheetah.Core.Dapper

Provider-independent Dapper data-access layer for Cheetah. Implements the shared
`IRepository<TEntity,TKey>` / `IReadOnlyRepository<TEntity,TKey>` contracts (from
`Cheetah.Core.DataAccess`) on top of Dapper, plus a fast raw-SQL path for read models.

It is designed to **coexist with the EF Core stack**: typically EF Core stays on the write
side (change tracking, domain events, migrations) and Dapper accelerates hot read paths — but
this module is a full DAL (read **and** write) and can be the sole data layer for a service.

## What it provides

| Type | Role |
|------|------|
| `IDbConnectionFactory` | Opens ADO.NET connections by named connection string (resolved via `IConnectionStringResolver`). |
| `IDapperConnectionProvider` | Provider hook that creates the concrete connection (implemented per database package). |
| `ISqlDialect` / `SqlDialectBase` | Per-engine SQL differences (quoting, parameter prefix, pagination, key retrieval). |
| `IDapperEntityMap` / `DapperEntityMap<T>` | Fluent table/column mapping — the Dapper analogue of EF's `IEntityTypeConfiguration<T>`. |
| `IEntityMapRegistry` | Resolves & caches mappings; falls back to conventions when no explicit map exists. |
| `ISpecificationParser<SqlWhere>` (`ExpressionToSqlParser`) | Translates existing `Specification<T>` into a SQL `WHERE` + parameters. |
| `SqlSpecification<T>` | Base for specifications that supply raw SQL when an expression can't be translated. |
| `ISqlBuilder` | Generates & caches CRUD statement templates per entity. |
| `IDapperUnitOfWork` | Scoped unit of work: shared connection per DB, buffered writes, atomic `SaveChanges` (one transaction per connection). |
| `DapperRepository<TEntity,TKey>` | `IRepository` implementation: spec-based reads + buffered writes. |
| `IDapperQueryExecutor` | Low-level raw-SQL executor for projections/reports (fresh connection per call). |

## Dependencies

- `Cheetah.Core`, `Cheetah.Core.Domain`, `Cheetah.Core.Specification`
- `Cheetah.Core.DataAccess` (connection-string resolution, repository contracts, specifications)
- `Dapper`
- A **provider module** that registers `IDapperConnectionProvider` + `ISqlDialect`
  (e.g. `Cheetah.Core.Dapper.PostgreSql`).

## Wiring

Reference a provider module from your DataAccess module:

```csharp
[DependsOn(typeof(MyDomainModule))]
[DependsOn(typeof(CrmDapperPostgreSqlModule))]   // brings in CrmDapperModule + dialect
public partial class MyDataAccessModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
        context.Services.AddDapperRepository<Customer>();   // opt-in binding (won't clobber EF)
    }
}
```

Connection strings reuse the shared `CrmDbConnectionOptions` (`ConnectionStrings:Default`, etc.).
Annotate an entity with `[ConnectionStringName("MyModule")]` to target a non-default database.

## Mapping

Convention by default (table = pluralized type name, columns = scalar property names; navigation
and collection properties — including `DomainEvents` — are ignored). Override explicitly:

```csharp
[Export(LifetimeType.Singleton, typeof(IDapperEntityMap))]
public sealed class CustomerMap : DapperEntityMap<Customer>
{
    public CustomerMap()
    {
        ToTable("Customers", schema: "crm");
        HasKey(x => x.Id);                 // Guid keys => app-assigned; other keys => DB-generated
        Column(x => x.FullName, "full_name");
        Ignore(x => x.SomeTransientFlag);
    }
}
```

The `[Export(..., typeof(IDapperEntityMap))]` attribute makes the map discoverable by
`IEntityMapRegistry`.

## Specifications (mandatory for filtering)

Existing `Specification<T>` classes work unchanged — `ExpressionToSqlParser` translates the
expression to SQL. Supported: `&& || !`, the six comparison operators, member access, captured
constants, and `string.Contains/StartsWith/EndsWith` (→ `LIKE`); `null` → `IS [NOT] NULL`.

For anything not translatable, write a `SqlSpecification<T>`:

```csharp
public sealed class ActiveCustomersInRegionSpec : SqlSpecification<Customer>
{
    private readonly string _region;
    public ActiveCustomersInRegionSpec(string region) => _region = region;

    public override Expression<Func<Customer, bool>> ToExpression() => c => c.Region == _region; // for in-memory checks
    public override SqlWhere ToSql() => new()
    {
        Sql = "is_active = true AND region = @region",
        Parameters = new Dictionary<string, object?> { ["region"] = _region },
    };
}
```

## Write path (unit of work)

`DapperRepository` follows the same flow as the EF repository: `Add/Update/Delete` buffer
operations, and `SaveChangesAsync` flushes them inside a transaction (one per connection) and
commits atomically. Publish domain events **after** `SaveChangesAsync`, as elsewhere in Cheetah.

## Read path (raw SQL)

For grids/reports inject `IDapperQueryExecutor` into a query handler:

```csharp
var rows = await _db.QueryAsync<CustomerListItem>(
    "SELECT id, full_name AS \"FullName\" FROM \"Customers\" WHERE is_active = @active LIMIT @take OFFSET @skip",
    new { active = true, take = q.Take, skip = q.Skip }, cancellationToken: ct);
```

## Constraints

- `AsQueryable()` / `AsNoTrackingQueryable()` throw `NotSupportedException` — Dapper has no
  `IQueryable`. Use specifications or `IDapperQueryExecutor`.
- No migrations/seeding — schema is owned by the EF stack (or external DDL).
- Repository binding is **opt-in** via `AddDapperRepository<...>()`; the module does not globally
  override `IRepository<,>`.
