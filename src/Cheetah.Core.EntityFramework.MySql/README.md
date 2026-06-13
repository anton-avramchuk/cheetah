# Cheetah.Core.EntityFramework.MySql

MySQL-провайдер для [Cheetah.Core.EntityFramework](../Cheetah.Core.EntityFramework/README.md). Зависит от `Cheetah.Core` и `CrmEntityFrameworkModule`.

## Состав

| Тип | Назначение |
|-----|------------|
| `CrmEntityFrameworkMySqlModule` | Модуль провайдера |

> В отличие от PostgreSQL/MsSql/Sqlite, выделенного `Use*`-расширения над `CrmDbContextConfigurationContext` пока нет — настраивайте контекст стандартным `UseMySql(...)` из пакета провайдера (Pomelo/MySql.EntityFrameworkCore).

## Подключение

```csharp
[DependsOn(typeof(CrmEntityFrameworkModule))]
[DependsOn(typeof(CrmEntityFrameworkMySqlModule))]
public partial class MyDataAccessModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var cs = context.Services.GetConfiguration().GetConnectionString("MyModule");
        context.Services.AddDbContext<MyDbContext>(options =>
            options.UseMySql(cs, ServerVersion.AutoDetect(cs)));
    }
}
```
