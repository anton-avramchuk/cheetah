# Cheetah.Core.Dapper.PostgreSql

PostgreSQL provider for [`Cheetah.Core.Dapper`](../Cheetah.Core.Dapper/README.md). Supplies the
two provider hooks the core module needs:

| Type | Registers as | Role |
|------|--------------|------|
| `NpgsqlConnectionProvider` | `IDapperConnectionProvider` | Creates `NpgsqlConnection` instances. |
| `PostgreSqlDialect` | `ISqlDialect` | PostgreSQL syntax (`"`-quoted identifiers, `@` params, `LIMIT/OFFSET`, `RETURNING`). |

## Dependencies

- `Cheetah.Core.Dapper` (core abstractions; pulled in transitively as a module dependency)
- `Cheetah.Core`
- `Npgsql`

## Usage

Depend on this module from your DataAccess module — it transitively brings in `CrmDapperModule`:

```csharp
[DependsOn(typeof(MyDomainModule))]
[DependsOn(typeof(CrmDapperPostgreSqlModule))]
public partial class MyDataAccessModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
        context.Services.AddDapperRepository<Customer>();
    }
}
```

Connection strings come from the shared `CrmDbConnectionOptions`
(`ConnectionStrings:Default` or a name set via `[ConnectionStringName("...")]` on the entity).

## Notes

- The base ANSI behaviour in `SqlDialectBase` already matches PostgreSQL, so `PostgreSqlDialect`
  adds no overrides; it exists as the registered `ISqlDialect` for this provider.
- For PostgreSQL-specific CLR ⇆ column conversions (e.g. `jsonb`, enums, arrays), register Dapper
  type handlers (`SqlMapper.AddTypeHandler(...)`) during application startup.
