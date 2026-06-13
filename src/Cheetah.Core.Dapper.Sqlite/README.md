# Cheetah.Core.Dapper.Sqlite

SQLite provider for [`Cheetah.Core.Dapper`](../Cheetah.Core.Dapper/README.md). Useful for unit/
integration tests and local development.

| Type | Registers as | Role |
|------|--------------|------|
| `SqliteConnectionProvider` | `IDapperConnectionProvider` | Creates `Microsoft.Data.Sqlite.SqliteConnection` instances. |
| `SqliteDialect` | `ISqlDialect` | SQLite syntax (`"`-quoted identifiers, `@` params, `LIMIT/OFFSET`, `RETURNING`). |

## Dependencies

- `Cheetah.Core.Dapper`, `Cheetah.Core`
- `Microsoft.Data.Sqlite` (bundles a recent SQLite engine)

## Usage

```csharp
[DependsOn(typeof(MyDomainModule))]
[DependsOn(typeof(CrmDapperSqliteModule))]
public partial class MyDataAccessModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
        context.Services.AddDapperRepository<Customer>();
    }
}
```

## Notes

- `RETURNING` for database-generated keys requires SQLite ≥ 3.35 (satisfied by the bundled engine).
  Cheetah entities usually use application-assigned `Guid` keys, so this path is rarely hit.
- For an in-memory test database, keep one open connection alive for the lifetime of the test
  (e.g. `Data Source=:memory:`), since SQLite drops an in-memory DB when its last connection closes.
