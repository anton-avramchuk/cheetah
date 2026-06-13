# Cheetah.Core.Dapper.MySql

MySQL/MariaDB provider for [`Cheetah.Core.Dapper`](../Cheetah.Core.Dapper/README.md), built on the
async-first [MySqlConnector](https://mysqlconnector.net/) driver.

| Type | Registers as | Role |
|------|--------------|------|
| `MySqlConnectionProvider` | `IDapperConnectionProvider` | Creates `MySqlConnector.MySqlConnection` instances. |
| `MySqlDialect` | `ISqlDialect` | MySQL syntax (`` ` `` identifiers, `@` params, `LIMIT/OFFSET`, `LAST_INSERT_ID()`). |

## Dependencies

- `Cheetah.Core.Dapper`, `Cheetah.Core`
- `MySqlConnector`

## Usage

```csharp
[DependsOn(typeof(MyDomainModule))]
[DependsOn(typeof(CrmDapperMySqlModule))]
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

- **Identifiers** are backtick-quoted (`` `Name` ``), with `` ` `` escaped by doubling.
- MySQL has no `RETURNING`; a database-generated key is read back via
  `SELECT LAST_INSERT_ID();` appended to the `INSERT`, executed on the same
  connection/transaction. This applies to `AUTO_INCREMENT` integer keys; the common Cheetah
  `Guid`-key convention is application-assigned and does not use this path.
- Enable `AllowLoadLocalInfile`/multi-statements only if your workload needs them; the default
  connection string is sufficient for the generated CRUD.
