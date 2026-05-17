# Cheetah.Core.Events

Core-абстракции событийной шины. Зависимостей минимум — только `Cheetah.Core`. Конкретные транспорты подключаются отдельными модулями:

- [Cheetah.Backend.Events.Redis](../Cheetah.Backend.Events.Redis/README.md) — Redis pub/sub, обычно дефолтный `IEventBus`
- [Cheetah.Backend.Events.InMemory](../Cheetah.Backend.Events.InMemory/README.md) — in-process для тестов/dev
- [Cheetah.Backend.Events.Kafka](../Cheetah.Backend.Events.Kafka/README.md) — publish-only Kafka через keyed `IEventBus(EventBusKeys.Kafka)`

## Состав

| Тип | Назначение |
|-----|------------|
| `IEvent` | Маркер-интерфейс события с `EventId` и `OccurredAt` |
| `EventBase` | Базовый record для событий, генерирует `EventId` и `OccurredAt` |
| `IEventBus` | `PublishAsync`, `PublishManyAsync`, `Subscribe<TEvent, THandler>` |
| `IEventHandler<TEvent>` | Контракт обработчика события |
| `EventBusKeys` | Стандартные ключи keyed-регистрации: `Redis`, `Kafka`, `InMemory` |

## EventBusKeys

Используется когда в приложении одновременно подключено несколько транспортов:

```csharp
[DependsOn(typeof(CrmBackendEventsRedisModule))]   // default IEventBus + keyed("redis")
[DependsOn(typeof(CrmBackendEventsKafkaModule))]   // keyed("kafka")
public partial class MyAppModule : CrmModule { }
```

```csharp
public class CustomerHandler(IEventBus bus) { }  // дефолт = Redis

public class AnalyticsPublisher(
    [FromKeyedServices(EventBusKeys.Kafka)] IEventBus kafka) { }
```

Это решает проблему "как иметь 2 транспорта одновременно" без введения generic-параметров типа `IEventBus<TTransport>` и без двусмысленности "какой bus default".

## Когда писать своё событие vs реюзать

Внутри Cheetah-модулей предпочтительно использовать `EventBase`:

```csharp
public sealed record CustomerCreatedEvent : EventBase
{
    public required Guid CustomerId { get; init; }
    public required string Name { get; init; }
}
```

`EventId` и `OccurredAt` проставятся автоматически, дедупликация на downstream (через Cheetah.Core.Outbox.IInboxStore) работает по `EventId` без дополнительной возни.
