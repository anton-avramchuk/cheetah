# Cheetah.Frontend.Events

In-memory реализация Event Bus для Blazor frontend приложений.

## Описание

Этот модуль предоставляет in-memory реализацию `IEventBus` для использования в Blazor приложениях. В отличие от backend реализации с Redis, события обрабатываются синхронно в памяти в рамках одного процесса.

## Основные компоненты

### CrmInMemoryEventBus

In-memory реализация `IEventBus`:
- **Singleton** - единственный экземпляр на все приложение
- **Thread-safe** - использует lock для безопасной работы с подписками
- **Синхронное выполнение** - все обработчики выполняются в одном процессе
- **Обработка ошибок** - продолжает выполнение других обработчиков даже если один упал

## Использование

### Регистрация модуля

Модуль автоматически регистрируется через зависимость:

```csharp
[DependsOn(typeof(CrmFrontendEventsModule))]
public class MyBlazorModule : CrmModule
{
}
```

### Публикация событий

```csharp
public class MyComponent
{
    private readonly IEventBus _eventBus;

    public MyComponent(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task OnButtonClick()
    {
        var @event = new UserClickedEvent { UserId = 123 };
        await _eventBus.PublishAsync(@event);
    }
}
```

### Подписка на события

```csharp
public class MyBlazorModule : CrmModule
{
    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var eventBus = context.ServiceProvider.GetRequiredService<IEventBus>();
        eventBus.Subscribe<UserClickedEvent, UserClickedEventHandler>();
    }
}
```

### Обработчик событий

```csharp
[Export(LifetimeType.Scoped, typeof(UserClickedEventHandler))]
public class UserClickedEventHandler : IEventHandler<UserClickedEvent>
{
    public async ValueTask HandleAsync(UserClickedEvent @event, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"User {event.UserId} clicked!");
        await Task.CompletedTask;
    }
}
```

## Отличия от Backend.Events.Redis

| Характеристика | Frontend (In-Memory) | Backend (Redis) |
|----------------|---------------------|-----------------|
| Транспорт | В памяти процесса | Redis Pub/Sub |
| Область действия | Один процесс | Несколько процессов |
| Производительность | Очень быстро | Зависит от сети |
| Персистентность | Нет | Нет (pub/sub) |
| Использование | Blazor UI события | Распределенные события |

## Архитектура

```
┌─────────────────┐
│  Blazor Component│
└────────┬────────┘
         │ PublishAsync
         ▼
┌─────────────────────────┐
│ CrmInMemoryEventBus     │
│  (Singleton)            │
└────────┬────────────────┘
         │
         ├─► Handler 1 (Scoped)
         ├─► Handler 2 (Scoped)
         └─► Handler N (Scoped)
```

## Примеры использования

### 1. Обновление UI компонента при событии

```csharp
// Event
public record DataChangedEvent : EventBase
{
    public int EntityId { get; init; }
}

// Component
@inject IEventBus EventBus

@code {
    protected override void OnInitialized()
    {
        EventBus.Subscribe<DataChangedEvent, DataChangedHandler>();
    }
}

// Handler
public class DataChangedHandler : IEventHandler<DataChangedEvent>
{
    public async ValueTask HandleAsync(DataChangedEvent @event, CancellationToken ct)
    {
        // Обновить UI
        await InvokeAsync(StateHasChanged);
    }
}
```

### 2. Пакетная публикация событий

```csharp
var events = items.Select(item => new ItemCreatedEvent { ItemId = item.Id });
await _eventBus.PublishManyAsync(events);
```

## Thread Safety

`CrmInMemoryEventBus` является thread-safe:
- Подписки защищены lock
- Обработчики выполняются в отдельном DI scope
- Множественные handler'ы могут регистрироваться на одно событие

## Производительность

In-memory event bus оптимизирован для Blazor приложений:
- ✅ Минимальные накладные расходы
- ✅ Синхронное выполнение в том же процессе
- ✅ Использует ValueTask для минимизации аллокаций
- ✅ Lock-free чтение списка обработчиков (копия создается под lock)

## Ограничения

- События не персистентны (пропадают при перезапуске приложения)
- Работает только в рамках одного процесса
- Не подходит для распределенных сценариев
- Обработчики выполняются последовательно (не параллельно)

## Зависимости

- `Cheetah.Core` - базовая функциональность
- `Cheetah.Core.Events` - интерфейсы событий

## См. также

- [Cheetah.Backend.Events.Redis](../Cheetah.Backend.Events.Redis/README.md) - реализация для backend
- [Cheetah.Core.Events](../Cheetah.Core.Events/README.md) - базовые интерфейсы
