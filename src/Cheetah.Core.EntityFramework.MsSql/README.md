# Cheetah.Core.EntityFramework.MsSql

SQL Server-провайдер для [Cheetah.Core.EntityFramework](../Cheetah.Core.EntityFramework/README.md). Альтернатива PostgreSQL (дефолт проекта). Зависит от `Cheetah.Core` и `CrmEntityFrameworkModule`.

## Состав

| Тип | Назначение |
|-----|------------|
| `CrmEntityFrameworkMsSqlModule` | Модуль провайдера |
| `UseSqlServer(...)` (расширение `CrmDbContextConfigurationContext`) | Настройка контекста на SQL Server |

## Подключение

```csharp
[DependsOn(typeof(CrmEntityFrameworkModule))]
[DependsOn(typeof(CrmEntityFrameworkMsSqlModule))]
public partial class MyDataAccessModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddDbContext<MyDbContext>(options =>
            options.UseSqlServer(context.Services.GetConfiguration().GetConnectionString("MyModule")));
    }
}
```
