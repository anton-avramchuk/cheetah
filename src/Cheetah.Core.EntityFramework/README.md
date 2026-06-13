# Cheetah.Core.EntityFramework

EF Core реализация абстракций `Cheetah.Core.DataAccess`: базовый `DbContext`, generic-репозиторий, авто-миграции и сидинг при старте. Провайдеро-независим — конкретная СУБД подключается отдельным модулем-провайдером. Зависит от `Cheetah.Core`, `Cheetah.Core.Domain`, `Cheetah.Core.DataAccess`.

Провайдеры:
- [Cheetah.Core.EntityFramework.PostgreSql](../Cheetah.Core.EntityFramework.PostgreSql/README.md) — `UseNpgsql()` (дефолт проекта)
- [Cheetah.Core.EntityFramework.MsSql](../Cheetah.Core.EntityFramework.MsSql/README.md)
- [Cheetah.Core.EntityFramework.MySql](../Cheetah.Core.EntityFramework.MySql/README.md)
- [Cheetah.Core.EntityFramework.Sqlite](../Cheetah.Core.EntityFramework.Sqlite/README.md)
- [Cheetah.Core.EntityFramework.Tenants](../Cheetah.Core.EntityFramework.Tenants/README.md) — мультитенантность (БД-на-тенанта)

## Состав

| Область | Типы | Назначение |
|---------|------|------------|
| DbContext | `CrmDbContext`, `ICrmDbContext`, `CrmDbContextOptions`, `[ReplaceDbContext]` | Базовый контекст и его настройка |
| Репозиторий | `EfRepository<TDbContext, TEntity, TKey>` | Реализация `IRepository<,>` поверх EF |
| Configuration | `EntityConfiguration<T>`, `AggregateRootConfiguration<T>` | Базы для `IEntityTypeConfiguration` |
| Migrations | `DatabaseMigrationManager`, `IDatabaseMigrator`, `EfCoreMigrator` | Авто-применение миграций на старте |
| Seeding | `DatabaseSeedManager`, `IDatabaseSeeder` | Сидинг данных на старте |
| Providers | `IDbContextProvider<TDbContext>`, `DbContextProvider<>` | Получение настроенного контекста |

## Запуск

В `OnApplicationInitialization` модуль автоматически применяет все миграции (`DatabaseMigrationManager`) и выполняет сидинг (`DatabaseSeedManager`), если они зарегистрированы.

## Подключение в модуле DataAccess

```csharp
[DependsOn(typeof(MyDomainModule))]
[DependsOn(typeof(CrmEntityFrameworkModule))]
[DependsOn(typeof(CrmEntityFrameworkPostgreSqlModule))]
public partial class MyDataAccessModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
        context.Services.AddDbContext<MyDbContext>(options =>
            options.UseNpgsql(context.Services.GetConfiguration().GetConnectionString("MyModule")));
    }
}
```

```csharp
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.Ignore(e => e.DomainEvents); // ОБЯЗАТЕЛЬНО
    }
}
```
