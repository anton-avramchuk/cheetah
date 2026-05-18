# Cheetah.Core.Outbox.EntityFrameworkCore

EF Core реализация `IOutboxStore` для [Cheetah.Core.Outbox](../Cheetah.Core.Outbox/README.md).

> **Inbox** живёт в отдельном модуле — см. [`Cheetah.Core.Inbox.EntityFrameworkCore`](../Cheetah.Core.Inbox.EntityFrameworkCore/README.md).

## Состав

| Тип | Назначение |
|-----|------------|
| `IOutboxDbContext` | Маркер DbContext'a с `DbSet<OutboxMessage> OutboxMessages` |
| `EfOutboxStore<TContext>` | EF Core реализация `IOutboxStore` |
| `OutboxMessageConfiguration` | EF-конфигурация + индекс `IX_OutboxMessages_Pending` (`ProcessedAt`, `NextAttemptAt`, `OccurredAt`) |
| `ModelBuilder.AddOutbox()` | Расширение для `OnModelCreating` |
| `services.AddOutboxStore<TContext>()` / `AddDeadLetterStore<TContext>()` | DI-расширения |
| `IDeadLetterDbContext` / `EfDeadLetterStore<TContext>` / `DeadLetterMessageConfiguration` / `modelBuilder.AddDeadLetter()` | EF Core реализация DLQ |
| `CrmOutboxEntityFrameworkCoreModule` | Зависит от `CrmEntityFrameworkModule` + `CrmOutboxModule` |

## Подключение

### 1. DbContext

```csharp
public class MyDbContext : CrmDbContext<MyDbContext>, IOutboxDbContext
{
    public DbSet<MyEntity> MyEntities { get; set; } = null!;
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;

    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new MyEntityConfiguration());

        modelBuilder.AddOutbox();   // ← таблица OutboxMessages
        // Для inbox используйте отдельный модуль Cheetah.Core.Inbox.EntityFrameworkCore.
    }
}
```

### 2. Модуль

```csharp
[DependsOn(typeof(MyDomainModule))]
[DependsOn(typeof(CrmEntityFrameworkModule))]
[DependsOn(typeof(CrmEntityFrameworkPostgreSqlModule))]
[DependsOn(typeof(CrmBackendEventsRedisModule))]
[DependsOn(typeof(CrmOutboxModule))]
[DependsOn(typeof(CrmOutboxEntityFrameworkCoreModule))]
public partial class MyDataAccessModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);

        context.Services.AddDbContext<MyDbContext>(options =>
            options.UseNpgsql(context.Services.GetConfiguration().GetConnectionString("MyModule")));

        context.Services.AddOutboxStore<MyDbContext>();
    }
}
```

### 3. Миграции

После `AddOutbox()` в `OnModelCreating` нужно сгенерировать миграцию:

```bash
dotnet ef migrations add AddOutbox --project src/Modules/MyModule/MyDataAccess
```

## Схема таблиц

**OutboxMessages**
| Колонка | Тип | Назначение |
|---------|-----|------------|
| `Id` | `uuid` PK | |
| `EventType` | `varchar(512)` | `AssemblyQualifiedName` события |
| `Payload` | `text` | JSON |
| `OccurredAt` | `timestamptz` | |
| `ProcessedAt` | `timestamptz?` | null — не обработано |
| `RetryCount` | `int` | |
| `NextAttemptAt` | `timestamptz?` | следующая попытка после ошибки |
| `Error` | `varchar(4000)?` | последняя ошибка |

Индекс `IX_OutboxMessages_Pending(ProcessedAt, NextAttemptAt, OccurredAt)` — для горячего запроса processor'а.

## Multi-DbContext

Каждый модуль со своим DbContext должен:
- реализовать `IOutboxDbContext` на своём DbContext'e;
- вызвать `services.AddOutboxStore<MyDbContext>()`.

Если в приложении несколько таких регистраций — последняя выигрывает (DI). Для одновременной работы нескольких outbox'ов используй отдельные `OutboxProcessor` на каждый контекст — это потребует расширения текущей схемы (в roadmap).
