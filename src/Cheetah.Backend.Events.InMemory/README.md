# Cheetah.Backend.Events.InMemory

In-process реализация `IEventBus` из [Cheetah.Core.Events](../Cheetah.Core.Events/README.md). События обрабатываются синхронно в том же процессе, без внешнего брокера. Предназначена для тестов и dev-окружения. Прод-транспорт — [Redis](../Cheetah.Backend.Events.Redis/README.md).

## Состав

| Тип | Назначение |
|-----|------------|
| `InMemoryEventBus` (`IEventBus`, Singleton) | `PublishAsync`, `PublishManyAsync`, `Subscribe<TEvent, THandler>` |
| `CrmBackendEventsInMemoryModule` | Модуль |

## Поведение

- Подписки хранятся в памяти (`Dictionary<Type, List<Type>>`), потокобезопасно.
- При публикации диспетчеризация идёт по **фактическому типу** события (`@event.GetType()`), а не по выводимому `TEvent`. Это важно: при публикации через абстракцию (`IEnumerable<IEvent>` из `aggregate.DomainEvents`) `TEvent` выводится как `IEvent`, и без этого подписки на конкретные типы не находились бы.
- Каждый handler резолвится в своём DI-scope.

## Подключение

```csharp
[DependsOn(typeof(CrmBackendEventsInMemoryModule))]
public partial class TestAppModule : CrmModule { }
```

```csharp
eventBus.Subscribe<OrderCreatedEvent, OrderCreatedEventHandler>();
await eventBus.PublishAsync(new OrderCreatedEvent(id, number), ct);
```
