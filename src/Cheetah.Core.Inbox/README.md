# Cheetah.Core.Inbox

Идемпотентная обработка входящих событий (Inbox pattern). Парный к `Cheetah.Core.Outbox`:
если Outbox гарантирует **публикацию ровно один раз** из локальной БД в брокер,
то Inbox гарантирует **обработку ровно один раз** при доставке из брокера.

Критично для микросервисов с at-least-once доставкой (Kafka, Redis Streams):
без Inbox любой retry на стороне брокера приводит к двойной обработке события.

## Что входит в модуль

- `IInboxStore` — хранилище записей `(EventId, ConsumerName)`.
- `InboxMessage` — запись об обработанном событии (PK `(EventId, ConsumerName)`).
- `InboxIdempotentEventHandler<TEvent>` — декоратор поверх `IEventHandler<TEvent>`:
  проверяет inbox перед вызовом inner-handler'а, фиксирует запись после успеха.
- `IdempotentAttribute` — маркер для авто-обёртки handler'а через Source Generator.
- `IdempotentHandlerRegistration.AddIdempotentHandler<TEvent, THandler>` — ручная регистрация.
- `InboxOptions` — retention, cleanup-интервал, размер batch'а.
- `InboxCleanupService` — фоновый сервис, чистит таблицу по retention.
- `IInboxMetrics` — точки метрик (null-провайдер по умолчанию).

## Подключение

### 1. Объявить DbContext модуля

DbContext должен реализовать `IInboxDbContext` и подключить таблицу через `AddInbox()`.
Это даёт пакет `Cheetah.Core.Inbox.EntityFrameworkCore`:

```csharp
using Cheetah.Core.Inbox;
using Cheetah.Core.Inbox.EntityFrameworkCore;

public class MyDbContext : DbContext, IInboxDbContext
{
    public DbSet<InboxMessage> InboxMessages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.AddInbox();
    }
}
```

### 2. Зарегистрировать модули и store

```csharp
[DependsOn(typeof(CrmInboxModule))]
[DependsOn(typeof(CrmInboxEntityFrameworkCoreModule))]
public partial class MyModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;
        services.AddDbContext<MyDbContext>(opts => opts.UseNpgsql(...));
        services.AddInboxStore<MyDbContext>();
    }
}
```

Для production-нагрузки в Postgres добавьте миграцию с
`InboxOptimizedIndexSql.Create()` — это создаст индекс по `ReceivedAt`
для эффективного cleanup.

### 3. Пометить event-handler как идемпотентный

```csharp
[Idempotent]
[Export(LifetimeType.Scoped, typeof(IEventHandler<OrderCreatedEvent>))]
public class OrderCreatedEventHandler : IEventHandler<OrderCreatedEvent>
{
    public ValueTask HandleAsync(OrderCreatedEvent @event, CancellationToken ct)
    {
        // ... ваш код. Source Generator оборачивает этот handler
        //     в InboxIdempotentEventHandler автоматически.
    }
}
```

Source Generator `Cheetah.Generators.Module` найдёт `[Idempotent]`-помеченные
классы с `[Export]` и сгенерирует регистрацию через декоратор.

### 4. Конфигурация

```json
{
  "Inbox": {
    "RetentionPeriod": "30.00:00:00",
    "CleanupInterval": "01:00:00",
    "CleanupBatchSize": 1000
  }
}
```

## Атомарность

`InboxIdempotentEventHandler<TEvent>` добавляет запись в inbox через `IInboxStore.AddAsync`,
**но не вызывает `SaveChangesAsync`** — это делает прикладной код handler'а.
Это позволяет фиксировать запись inbox **в той же транзакции**, что и бизнес-эффекты,
что и есть ключ к идемпотентности: либо handler сработал и inbox-запись есть, либо ни того, ни другого.

⚠️ Inner-handler **должен** использовать тот же DbContext, что зарегистрирован
как `IInboxDbContext`. Если у вас разные scope'ы — атомарность теряется.

## Совместимость монолит ↔ микросервисы

- В **монолите** Inbox защищает от случайной двойной публикации (например, при retry'ах
  в Redis EventBus).
- В **микросервисах** Inbox — обязательное условие корректности при at-least-once Kafka.

## Связанные сборки

- `Cheetah.Core.Inbox.EntityFrameworkCore` — EF Core реализация `IInboxStore`.
- `Cheetah.Core.Inbox.PostgreSql` — Postgres-оптимизированные индексы.
- `Cheetah.Core.Outbox` — парный модуль для гарантированной публикации.
