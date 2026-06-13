# Cheetah.Core.EntityFramework.Sqlite

SQLite-провайдер для [Cheetah.Core.EntityFramework](../Cheetah.Core.EntityFramework/README.md). Удобен для локальной разработки, тестов и embedded-сценариев. Зависит от `Cheetah.Core` и `CrmEntityFrameworkModule`.

## Состав

| Тип | Назначение |
|-----|------------|
| `CrmEntityFrameworkSqliteModule` | Модуль провайдера |
| `UseSqlite(...)` (расширение `CrmDbContextConfigurationContext`) | Настройка контекста на SQLite |

## Подключение

```csharp
[DependsOn(typeof(CrmEntityFrameworkModule))]
[DependsOn(typeof(CrmEntityFrameworkSqliteModule))]
public partial class MyDataAccessModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddDbContext<MyDbContext>(options =>
            options.UseSqlite(context.Services.GetConfiguration().GetConnectionString("MyModule")));
    }
}
```
