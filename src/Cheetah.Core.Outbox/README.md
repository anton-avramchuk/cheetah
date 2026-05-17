# Cheetah.Core.Outbox

Реализация **Outbox-паттерна** для Cheetah CRM. Гарантирует, что доменное событие будет опубликовано тогда и только тогда, когда бизнес-транзакция успешно зафиксирована в БД.

## Зачем

Классическая проблема: handler меняет агрегат → `SaveChangesAsync()` → `_eventBus.PublishAsync(event)`. Если процесс упадёт между SaveChanges и Publish — событие потеряется. Если упадёт после Publish, но до SaveChanges — событие уйдёт без бизнес-изменений.

Outbox решает это, превращая публикацию в обычный INSERT в ту же транзакцию, что и агрегат. Фоновый процесс читает таблицу и шлёт события в реальный транспорт (Redis/InMemory) с retry и backoff.

## Архитектура

```
CommandHandler
    │
    │  _eventBus.PublishAsync(event)        ← IEventBus = OutboxEventBus (Scoped)
    ▼
OutboxEventBus  ──►  IOutboxStore.AddAsync()       (одна транзакция с агрегатом)
                          │
                          ▼
                    OutboxMessages table
                          ▲
                          │  GetPendingAsync(batch)
OutboxProcessor (BackgroundService, polling)
    │
    │  PublishAsync<TEvent>(event)          ← через IInnerEventBus
    ▼
CrmRedisEventBus (или другой транспорт)
```

## Состав

| Тип | Назначение |
|-----|------------|
| `IOutboxStore` | Хранилище outbox-сообщений (реализуется отдельным модулем) |
| `IInboxStore` | Хранилище обработанных EventId — для идемпотентности consumer'ов |
| `OutboxMessage` / `InboxMessage` | Entity-записи |
| `OutboxEventBus` | Scoped-декоратор `IEventBus`, пишет в outbox вместо немедленной отправки |
| `IInnerEventBus` | Адаптер над реальным транспортом, который зовёт processor |
| `OutboxProcessor` | `BackgroundService` с polling + сигнал от `IOutboxNotifier` (что раньше) |
| `OutboxCleanupService` | `BackgroundService`: удаляет обработанные `OutboxMessages` и старые `InboxMessages` старше `RetentionPeriod` (default 7 дней), batch'ами |
| `IDeadLetterStore` / `DeadLetterMessage` | DLQ: при превышении `MaxRetries` сообщение переезжает в `DeadLetterMessages` (если store зарегистрирован), освобождая горячую таблицу. `RequeueAsync` возвращает обратно с обнулённым `RetryCount` |
| `IOutboxMetrics` / `NullOutboxMetrics` | Абстракция метрик. По умолчанию — no-op. Для OpenTelemetry-экспорта подключите отдельный модуль `Cheetah.Core.Outbox.OpenTelemetry` |
| `IOutboxNotifier` | Источник пробуждения processor'а. По умолчанию — `NullOutboxNotifier` (только polling). См. `Cheetah.Core.Outbox.PostgreSql` для LISTEN/NOTIFY |
| `InboxIdempotentEventHandler<TEvent>` | Декоратор IEventHandler: проверка `IInboxStore.AlreadyProcessedAsync` до вызова + запись `InboxMessage` после |
| `[Idempotent]` | Атрибут-маркер; Source Generator оборачивает помеченные `IEventHandler<TEvent>` в `InboxIdempotentEventHandler<TEvent>` автоматически |
| `OutboxOptions` | `BatchSize`, `PollingInterval`, `MaxRetries`, `BaseRetryDelay`, `MaxRetryDelay` |
| `CrmOutboxModule` | Перехватывает регистрацию `IEventBus`, переносит её на `IInnerEventBus`, регистрирует `OutboxEventBus` и `OutboxProcessor` |

## Подключение

1. Зарегистрируй транспорт (Redis / InMemory) **до** Outbox-модуля:
   ```csharp
   [DependsOn(typeof(CrmBackendEventsRedisModule))]
   [DependsOn(typeof(CrmOutboxModule))]
   [DependsOn(typeof(CrmOutboxEntityFrameworkCoreModule))]
   public partial class CrmMyModule : CrmModule { }
   ```

2. Подключи реализацию store — обычно через `Cheetah.Core.Outbox.EntityFrameworkCore`. См. README этого проекта.

3. Конфигурация (`appsettings.json`):
   ```json
   "Outbox": {
     "BatchSize": 100,
     "PollingInterval": "00:00:02",
     "MaxRetries": 10,
     "BaseRetryDelay": "00:00:05",
     "MaxRetryDelay": "00:10:00",
     "RetentionPeriod": "7.00:00:00",
     "CleanupInterval": "01:00:00",
     "CleanupBatchSize": 1000
   }
   ```

4. (опционально) Подключи метрики через отдельный модуль `Cheetah.Core.Outbox.OpenTelemetry` — см. его README.

## Использование в коде

**Ничего не меняется.** Существующие хендлеры продолжают работать:

```csharp
public async ValueTask<Guid> HandleAsync(CreateMyEntityCommand cmd, CancellationToken ct)
{
    var entity = MyEntity.Create(cmd.Name);
    _repository.Add(entity);

    foreach (var e in entity.DomainEvents)
        await _eventBus.PublishAsync(e, ct);      // ← теперь идёт в outbox
    entity.ClearDomainEvents();

    await _repository.SaveChangesAsync(ct);       // ← коммит агрегата + outbox атомарно
    return entity.Id;
}
```

`IEventBus.Subscribe<>()` по-прежнему регистрирует подписку на реальном транспорте (делегируется в `IInnerEventBus`).

## Tradeoffs

- **+** Гарантия "если бизнес-операция прошла, событие будет опубликовано".
- **+** Никаких изменений в существующих хендлерах.
- **−** Доставка стала eventual (задержка ≈ `PollingInterval`).
- **−** Дополнительный write на каждое событие.
- **−** Polling-based — под 10k RPS таблица станет горячей. Решение: `LISTEN/NOTIFY` Postgres (в roadmap).

## Идемпотентность consumer'а

Outbox защищает publisher'а. Для consumer'а используй декоратор `InboxIdempotentEventHandler<TEvent>`:

```csharp
// в ConfigureServices модуля:
services.AddIdempotentHandler<MyEvent, MyEventHandler>();
```

или маркер `[Idempotent]` (требует Source Generator поддержки в `Cheetah.Generators.Application`):

```csharp
[Idempotent]
public class MyEventHandler : IEventHandler<MyEvent>
{
    public async ValueTask HandleAsync(MyEvent e, CancellationToken ct)
    {
        // бизнес-логика + _context.SaveChangesAsync(ct)
        // Inbox-запись добавит декоратор, она зафиксируется в той же транзакции.
    }
}
```

**Важно:** хендлер сам зовёт `SaveChangesAsync` на том же DbContext, что использует `IInboxStore` — иначе атомарности нет.
