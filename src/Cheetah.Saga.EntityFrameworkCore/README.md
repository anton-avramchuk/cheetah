# Cheetah.Saga.EntityFrameworkCore

EF Core реализация `ISagaRepository` для [Cheetah.Saga](../Cheetah.Saga/README.md).

## Состав

| Тип | Назначение |
|-----|------------|
| `ISagaDbContext` | Маркер DbContext с `DbSet<SagaInstance> SagaInstances` |
| `EfSagaRepository<TContext>` | Реализация ISagaRepository с optimistic locking через поле `Version` (concurrency token) |
| `SagaInstanceConfiguration` | EF config + unique index `(SagaType, CorrelationKey)`, concurrency token |
| `modelBuilder.AddSagas()` | Extension для OnModelCreating |
| `services.AddEfSagaRepository<TContext>()` | DI |
| `CrmSagaEntityFrameworkCoreModule` | Зависит от `CrmSagaModule` |

## Подключение

### 1. DbContext

```csharp
public class MyDbContext : DbContext, ISagaDbContext
{
    public DbSet<MyAggregate> Aggregates { get; set; } = null!;
    public DbSet<SagaInstance> SagaInstances { get; set; } = null!;

    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.AddSagas();
    }
}
```

### 2. Модуль

```csharp
[DependsOn(typeof(CrmSagaModule))]
[DependsOn(typeof(CrmSagaEntityFrameworkCoreModule))]
public partial class MyDataAccessModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddDbContext<MyDbContext>(...);
        context.Services.AddEfSagaRepository<MyDbContext>();
        context.Services.AddSaga<OrderProcessingSaga>();
    }
}
```

### 3. Миграция

`modelBuilder.AddSagas()` создаст таблицу `SagaInstances` с:
- PK `Id` (Guid)
- Unique `(SagaType, CorrelationKey)` — гарантирует одна сага = одна instance
- Concurrency token `Version` — optimistic locking при concurrent updates

## Optimistic locking

`EfSagaRepository.SaveChangesAsync`:
1. Перед save увеличивает `Version` на всех modified instance
2. EF Core добавляет `WHERE Version = @original` к UPDATE
3. Если в БД уже другая Version (другая реплика успела изменить) — `DbUpdateConcurrencyException`
4. Преобразуется в `SagaConcurrencyException`

Application code должен ловить эту exception и **повторить обработку event'а** — обычно через retry на уровне event handler / outbox.

## Multi-instance

- Уникальный constraint на `(SagaType, CorrelationKey)` гарантирует, что две реплики не создадут две instance с одним correlation key. Одна получит `DbUpdateException` на INSERT, должна повторить и продолжить с уже созданной instance.
- Concurrency token на `Version` гарантирует, что два UPDATE одной instance не потеряют изменения.
