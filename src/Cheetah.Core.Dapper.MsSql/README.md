# Cheetah.Core.Dapper.MsSql

Microsoft SQL Server provider for [`Cheetah.Core.Dapper`](../Cheetah.Core.Dapper/README.md).

| Type | Registers as | Role |
|------|--------------|------|
| `SqlServerConnectionProvider` | `IDapperConnectionProvider` | Creates `Microsoft.Data.SqlClient.SqlConnection` instances. |
| `SqlServerDialect` | `ISqlDialect` | SQL Server syntax (`[ ]` identifiers, `@` params, `OFFSET/FETCH`, `OUTPUT INSERTED`). |

## Dependencies

- `Cheetah.Core.Dapper`, `Cheetah.Core`
- `Microsoft.Data.SqlClient`

## Usage

```csharp
[DependsOn(typeof(MyDomainModule))]
[DependsOn(typeof(CrmDapperMsSqlModule))]
public partial class MyDataAccessModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
        context.Services.AddDapperRepository<Customer>();
    }
}
```

## Dialect notes

- **Identifiers** are bracket-quoted (`[Name]`), with `]` escaped as `]]`.
- **Pagination** uses `OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY`, which **requires an
  `ORDER BY`** in the query you pass to `ISqlDialect.Paginate` / `IDapperQueryExecutor`.
- **Generated keys** are returned via an `OUTPUT INSERTED.[key]` clause injected before `VALUES`.
  Cheetah entities typically use application-assigned `Guid` keys, so this path is rarely used.
