# Cheetah.Core.Inbox.EntityFrameworkCore

EF Core реализация `IInboxStore` для [Cheetah.Core.Inbox](../Cheetah.Core.Inbox/README.md).

## Состав

| Тип | Назначение |
|-----|------------|
| `IInboxDbContext` | Маркер DbContext'a с `DbSet<InboxMessage> InboxMessages` |
| `EfInboxStore<TContext>` | EF Core реализация `IInboxStore` |
| `InboxMessageConfiguration` | EF-конфигурация (составной PK `EventId + ConsumerName`, индекс `IX_InboxMessages_ReceivedAt`) |
| `ModelBuilder.AddInbox()` | Расширение для `OnModelCreating` |
| `services.AddInboxStore<TContext>()` | DI-расширение |
| `CrmInboxEntityFrameworkCoreModule` | Зависит от `CrmEntityFrameworkModule` + `CrmInboxModule` |

## Подключение

### 1. DbContext

```csharp
public class MyDbContext : CrmDbContext<MyDbContext>, IInboxDbContext
{
    public DbSet<MyEntity> MyEntities { get; set; } = null!;
    public DbSet<InboxMessage> InboxMessages { get; set; } = null!;

    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.AddInbox();
    }
}
```

### 2. Модуль

```csharp
[DependsOn(typeof(MyDomainModule))]
[DependsOn(typeof(CrmEntityFrameworkModule))]
[DependsOn(typeof(CrmEntityFrameworkPostgreSqlModule))]
[DependsOn(typeof(CrmInboxModule))]
[DependsOn(typeof(CrmInboxEntityFrameworkCoreModule))]
public partial class MyDataAccessModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);

        context.Services.AddDbContext<MyDbContext>(options =>
            options.UseNpgsql(context.Services.GetConfiguration().GetConnectionString("MyModule")));

        context.Services.AddInboxStore<MyDbContext>();
    }
}
```

### 3. Миграции

После `AddInbox()` в `OnModelCreating` нужно сгенерировать миграцию:

```bash
dotnet ef migrations add AddInbox --project src/Modules/MyModule/MyDataAccess
```

Для production Postgres-нагрузки добавьте в миграцию вызов
`Cheetah.Core.Inbox.PostgreSql.InboxOptimizedIndexSql.Create()` — он
создаст индекс по `ReceivedAt` для эффективного cleanup'а.

## Схема таблиц

**InboxMessages**
| Колонка | Тип | Назначение |
|---------|-----|------------|
| `EventId` | `uuid` | PK часть 1 |
| `ConsumerName` | `varchar(256)` | PK часть 2 |
| `EventType` | `varchar(512)` | `AssemblyQualifiedName` события |
| `ReceivedAt` | `timestamptz` | для cleanup'а |

Составной PK `(EventId, ConsumerName)` — также защищает от race condition'ов
(параллельные транзакции, пытающиеся записать обработку одного события одним consumer'ом —
одна получит unique violation).
