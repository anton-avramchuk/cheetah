# Cheetah.Core.EntityFramework.PostgreSql

PostgreSQL-провайдер для [Cheetah.Core.EntityFramework](../Cheetah.Core.EntityFramework/README.md). Дефолтная СУБД проекта. Зависит от `Cheetah.Core` и `CrmEntityFrameworkModule`.

## Состав

| Тип | Назначение |
|-----|------------|
| `CrmEntityFrameworkPostgreSqlModule` | Модуль провайдера |
| `UseNpgsql(...)` (расширение `CrmDbContextConfigurationContext`) | Настройка контекста на Npgsql; включает `QuerySplittingBehavior.SplitQuery`, поддерживает существующее соединение (`ExistingConnection`) |
| `NpgsqlConnectionStringChecker` | Проверка доступности БД по строке подключения |

> `UsePostgreSql(...)` устарел — используйте `UseNpgsql(...)`.

## Подключение

```csharp
[DependsOn(typeof(CrmEntityFrameworkModule))]
[DependsOn(typeof(CrmEntityFrameworkPostgreSqlModule))]
public partial class MyDataAccessModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddDbContext<MyDbContext>(options =>
            options.UseNpgsql(context.Services.GetConfiguration().GetConnectionString("MyModule")));
    }
}
```
