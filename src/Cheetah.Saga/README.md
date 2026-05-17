# Cheetah.Saga

Лёгкий orchestration-based saga-фреймворк. Долгоиграющие процессы из нескольких шагов с компенсациями. Хранение — отдельный модуль ([Cheetah.Saga.EntityFrameworkCore](../Cheetah.Saga.EntityFrameworkCore/README.md)), доставка событий — через `IEventBus` (Redis/Kafka/Outbox — любой).

## Концепция

Сага — это **state machine, который живёт между несколькими событиями**:

```
OrderCreated → CreateInvoice → InvoiceCreated → ChargePayment → PaymentSucceeded → Complete
                                                                       ↓ (если failed)
                                                              CancelInvoice → Compensated
```

Каждое событие приходит в orchestrator, который:
1. Находит подходящие саги по типу события
2. Извлекает `CorrelationKey` из события (обычно AggregateId)
3. Загружает существующую `SagaInstance` или создаёт новую (если событие помечено `[SagaStartedBy]`)
4. Десериализует `Data`, вызывает `On(event)`-метод саги
5. Сериализует обратно, сохраняет с **оптимистичной блокировкой**

## Состав

| Тип | Назначение |
|-----|------------|
| `Saga<TData>` | Базовый класс. Наследник реализует `On(EventType)` методы и `GetCorrelation(IEvent)` |
| `SagaInstance` | Персистентное состояние одного запущенного экземпляра |
| `SagaStatus` | `Running`, `Completed`, `Compensating`, `Compensated`, `Failed` |
| `[SagaStartedBy(typeof(Event))]` | Этот event начинает новую сагу |
| `[SagaHandles(typeof(Event))]` | Этот event обрабатывается существующей сагой (без неё — игнор) |
| `ISagaRepository` | Хранилище. `FindAsync`, `AddAsync`, `SaveChangesAsync` с optimistic locking |
| `SagaConcurrencyException` | Бросается при конфликте оптимистичной блокировки |
| `SagaOrchestrator` | Резолвер: event → подходящие саги → load/dispatch/save |
| `SagaEventHandler<TEvent>` | Generic-обёртка `IEventHandler<TEvent>` — автоматически регистрируется через `AddSaga` |
| `SagaRegistry` | Реестр зарегистрированных типов саг |
| `services.AddSaga<TSaga>()` | Регистрирует сагу + auto-wiring через `IEventHandler<T>` для каждого события из атрибутов |
| `CrmSagaModule` | Core-модуль; регистрирует `SagaOrchestrator` |

## Пример

```csharp
public sealed record OrderCreatedEvent(Guid OrderId) : EventBase;
public sealed record InvoiceCreatedEvent(Guid OrderId, Guid InvoiceId) : EventBase;
public sealed record PaymentSucceededEvent(Guid OrderId) : EventBase;
public sealed record PaymentFailedEvent(Guid OrderId, string Reason) : EventBase;

public class OrderSagaData
{
    public Guid OrderId { get; set; }
    public Guid? InvoiceId { get; set; }
}

[SagaStartedBy(typeof(OrderCreatedEvent))]
[SagaHandles(typeof(InvoiceCreatedEvent))]
[SagaHandles(typeof(PaymentSucceededEvent))]
[SagaHandles(typeof(PaymentFailedEvent))]
public class OrderProcessingSaga : Saga<OrderSagaData>
{
    private readonly IEventBus _bus;
    public OrderProcessingSaga(IEventBus bus) => _bus = bus;

    public override string GetCorrelation(IEvent @event) => @event switch
    {
        OrderCreatedEvent e => e.OrderId.ToString(),
        InvoiceCreatedEvent e => e.OrderId.ToString(),
        PaymentSucceededEvent e => e.OrderId.ToString(),
        PaymentFailedEvent e => e.OrderId.ToString(),
        _ => throw new InvalidOperationException()
    };

    public async ValueTask On(OrderCreatedEvent e, CancellationToken ct)
    {
        Data.OrderId = e.OrderId;
        await _bus.PublishAsync(new CreateInvoiceCommand(e.OrderId), ct);
    }

    public async ValueTask On(InvoiceCreatedEvent e, CancellationToken ct)
    {
        Data.InvoiceId = e.InvoiceId;
        await _bus.PublishAsync(new ChargePaymentCommand(e.InvoiceId), ct);
    }

    public ValueTask On(PaymentSucceededEvent e, CancellationToken ct)
    {
        Complete();
        return ValueTask.CompletedTask;
    }

    public async ValueTask On(PaymentFailedEvent e, CancellationToken ct)
    {
        await _bus.PublishAsync(new CancelInvoiceCommand(Data.InvoiceId!.Value), ct);
        Compensate(e.Reason);
    }
}
```

Регистрация:

```csharp
[DependsOn(typeof(CrmSagaModule))]
[DependsOn(typeof(CrmSagaEntityFrameworkCoreModule))]
public partial class MyAppModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddSaga<OrderProcessingSaga>();
        context.Services.AddEfSagaRepository<MyDbContext>();
    }
}
```

## Жизненный цикл

```
                       ┌───────────────────────────┐
                       │  Running                  │
                       │                           │
StartedBy event ──────►│  On(event) → Data mutation│
                       │                           │
Handles event ────────►│  ↓ может вызвать:         │
                       │    Complete() → Completed │
                       │    Compensate(r) ─────────┼──► Compensating
                       │    (unhandled exc) ───────┼──► Failed
                       └───────────────────────────┘
                                ▲
                                │ Handles event
                       ┌─────────────────┐
                       │  Compensating   │
                       │  On(event)      │
                       │    Complete() ──┼──► Compensated
                       └─────────────────┘

Terminal: Completed | Compensated | Failed — больше не реагирует на события.
```

## Атомарность и race conditions

- **Оптимистичная блокировка** через `SagaInstance.Version` (concurrency token EF Core). Если две реплики одновременно обрабатывают разные события одной саги — одна из них получит `SagaConcurrencyException` и должна повторить (через retry на уровне event handler).
- **Уникальный индекс** `(SagaType, CorrelationKey)` в EF-схеме предотвращает создание двух instance с одним correlation key.
- **DbUpdateException на unique constraint** при гонке двух `StartedBy` событий — orchestrator не делает retry, нужен механизм outside (например, `IOutboxStore`-retry на исходном event handler).

## Tradeoffs

- **+** Atomicity per-event. `On(event)` либо выполнился целиком и закоммитился, либо откатился.
- **+** Compatible с любым `IEventBus` (Redis/Kafka/Outbox).
- **+** Простой API — наследник `Saga<TData>` + атрибуты + `On(...)` методы. Никаких fluent state machine builders.
- **−** Reflection-based dispatch `On(EventType)` — на горячем пути ~200ns на event (кэш `MethodInfo` в `Saga<T>`).
- **−** Нет встроенного scheduler'а для timeout'ов вида "если PaymentSucceeded не пришло за 1 час — Compensate". Реализуется через отдельный BackgroundService, который шлёт `SagaTimedOut` events.
- **−** Полагается на user-provided `GetCorrelation` — нет автоматического correlation по convention (например, `event.OrderId`). Это сознательно: явное лучше неявного, плюс сценарии где correlation сложнее (composite key, parent saga).
