# Cheetah.Audit.EntityFrameworkCore

EF Core sink + publish store для [Cheetah.Audit](../Cheetah.Audit/README.md).

## Состав

| Тип | Назначение |
|-----|------------|
| `IAuditDbContext` | Маркер DbContext с `DbSet<AuditEntry> AuditEntries` |
| `EfAuditSink<TContext>` | IAuditSink, добавляет AuditEntry в ChangeTracker → попадает в одну транзакцию с агрегатом |
| `EfAuditPublishStore<TContext>` | IAuditPublishStore — для downstream publisher'ов (KafkaAuditPublisher) |
| `AuditEntryConfiguration` | EF-конфигурация + индексы IX_AuditEntries_Unpublished, IX_AuditEntries_Entity |
| `modelBuilder.AddAudit()` | Extension для OnModelCreating |
| `services.AddEfAuditSink<TContext>()` / `AddEfAuditPublishStore<TContext>()` | DI |
| `CrmAuditEntityFrameworkCoreModule` | Зависит от CrmAuditModule |

## Подключение

### 1. DbContext

```csharp
public class MyDbContext : DbContext, IAuditDbContext
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Customer>().HasKey(x => x.Id);
        modelBuilder.AddAudit();
    }
}
```

### 2. Регистрация в модуле

```csharp
[DependsOn(typeof(CrmAuditModule))]
[DependsOn(typeof(CrmAuditEntityFrameworkCoreModule))]
public partial class MyDataAccessModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddDbContext<MyDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString);
            options.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
        });

        context.Services.AddEfAuditSink<MyDbContext>();
        // Дополнительно если планируете публикацию в Kafka:
        context.Services.AddEfAuditPublishStore<MyDbContext>();
    }
}
```

### 3. Миграция

После `modelBuilder.AddAudit()` сгенерируйте миграцию — таблица AuditEntries появится автоматически с правильными индексами.

## Индексы

| Имя | Колонки | Зачем |
|-----|---------|-------|
| `IX_AuditEntries_Unpublished` | `(PublishedAt, OccurredAt)` | Горячий запрос KafkaAuditPublisher (WHERE PublishedAt IS NULL ORDER BY OccurredAt) |
| `IX_AuditEntries_Entity` | `(EntityType, EntityId, OccurredAt)` | UI истории изменений конкретной сущности |
