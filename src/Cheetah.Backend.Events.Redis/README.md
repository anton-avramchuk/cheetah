# Cheetah.Backend.Events.Redis

Реализация Event Bus на основе Redis для системы Cheetah. Связывает абстракцию `IEventBus` из `Cheetah.Core.Events` с реализацией Redis Pub/Sub из `Cheetah.Backend.Redis`.

## Возможности

- Реализация интерфейса `IEventBus`
- Публикация событий в Redis
- Подписка на события с автоматической маршрутизацией к хэндлерам
- Поддержка нескольких хэндлеров для одного события
- Обработка ошибок - падение одного хэндлера не блокирует другие
- Конфигурируемое имя Redis инстанса и префикс каналов

## DI-регистрация

`CrmBackendEventsRedisModule` регистрирует Redis-bus **двумя способами одновременно**:

1. **Дефолтная регистрация** (через `[Export]`) — `IEventBus` без ключа. Хендлеры без указания ключа получают именно её:
   ```csharp
   public class MyHandler(IEventBus bus) { } // → Redis
   ```

2. **Keyed-регистрация** `IEventBus(EventBusKeys.Redis)` — тот же singleton, доступен через явный ключ. Полезно когда в приложении подключён ещё один транспорт (например `Cheetah.Backend.Events.Kafka` с keyed `"kafka"`) и хочется быть явным:
   ```csharp
   public class AuditPublisher(
       [FromKeyedServices(EventBusKeys.Redis)] IEventBus redis,
       [FromKeyedServices(EventBusKeys.Kafka)] IEventBus kafka) { }
   ```

Обе регистрации указывают на один и тот же singleton, поэтому подписки видны независимо от способа резолва.

## Конфигурация

Добавьте в `appsettings.json`:

```json
{
  "RedisEventBus": {
    "InstanceName": "default",
    "ChannelPrefix": "events:"
  }
}
```

Параметры:
- **InstanceName** - имя инстанса Redis из конфигурации `Cheetah.Backend.Redis` (по умолчанию: "default")
- **ChannelPrefix** - префикс для имен каналов событий (по умолчанию: "events:")

## Использование

### 1. Определение события

```csharp
public record UserCreatedEvent : EventBase
{
    public int UserId { get; init; }
    public string Email { get; init; } = string.Empty;
}
```

### 2. Создание хэндлера

```csharp
[Export(LifetimeType.Scoped, typeof(IEventHandler<UserCreatedEvent>))]
public class SendWelcomeEmailHandler : IEventHandler<UserCreatedEvent>
{
    private readonly IEmailService _emailService;

    public SendWelcomeEmailHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async ValueTask HandleAsync(UserCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        await _emailService.SendWelcomeEmailAsync(@event.Email, cancellationToken);
    }
}
```

### 3. Регистрация подписки

В вашем модуле:

```csharp
public class MyModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var eventBus = context.Services.BuildServiceProvider().GetRequiredService<IEventBus>();
        eventBus.Subscribe<UserCreatedEvent, SendWelcomeEmailHandler>();
    }
}
```

### 4. Публикация событий

```csharp
public class UserService
{
    private readonly IEventBus _eventBus;

    public UserService(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task CreateUserAsync(CreateUserDto dto, CancellationToken ct)
    {
        // Создание пользователя
        var user = await _userRepository.CreateAsync(dto, ct);

        // Публикация события
        await _eventBus.PublishAsync(new UserCreatedEvent
        {
            UserId = user.Id,
            Email = user.Email
        }, ct);
    }

    public async Task CreateManyUsersAsync(List<CreateUserDto> dtos, CancellationToken ct)
    {
        var events = new List<UserCreatedEvent>();

        foreach (var dto in dtos)
        {
            var user = await _userRepository.CreateAsync(dto, ct);
            events.Add(new UserCreatedEvent
            {
                UserId = user.Id,
                Email = user.Email
            });
        }

        // Публикация нескольких событий
        await _eventBus.PublishManyAsync(events, ct);
    }
}
```

## Как это работает

1. **Публикация**: События сериализуются в JSON и публикуются в Redis канал с именем `{ChannelPrefix}{EventTypeName}`
   - Например: `events:UserCreatedEvent`

2. **Подписка**: При вызове `Subscribe<TEvent, THandler>()`:
   - Регистрируется подписка на Redis канал
   - Хэндлер сохраняется для последующего вызова
   - При получении события из Redis создается DI scope
   - Хэндлер резолвится из DI контейнера
   - Вызывается метод `HandleAsync` хэндлера

3. **Множественные хэндлеры**: Можно зарегистрировать несколько хэндлеров для одного события:
   ```csharp
   eventBus.Subscribe<UserCreatedEvent, SendWelcomeEmailHandler>();
   eventBus.Subscribe<UserCreatedEvent, CreateUserProfileHandler>();
   eventBus.Subscribe<UserCreatedEvent, LogUserCreationHandler>();
   ```
   Все хэндлеры будут вызваны при получении события.

4. **Обработка ошибок**: Если один хэндлер выбросит исключение, остальные продолжат выполнение.

## Архитектура

```
IEventBus (Core.Events)
    ↓
CrmRedisEventBus (Backend.Events.Redis)
    ↓
IRedisEventBus (Backend.Redis)
    ↓
Redis Pub/Sub
```

## Зависимости

- `Cheetah.Core.Events` - интерфейсы событий
- `Cheetah.Backend.Redis` - низкоуровневая работа с Redis
- `Cheetah.Core` - базовая инфраструктура

## Примечания

- События автоматически получают `EventId` (Guid) и `OccurredAt` (DateTimeOffset) от базового класса `EventBase`
- Хэндлеры должны быть зарегистрированы в DI контейнере с помощью атрибута `[Export]`
- События сериализуются/десериализуются автоматически с помощью System.Text.Json
- Каждый хэндлер выполняется в отдельном DI scope
- Подписки регистрируются только один раз для каждого типа события

## Тестирование

Смотрите `Cheetah.Backend.Redis.Tests/CrmRedisEventBusTests.cs` для примеров unit-тестов.

Результаты тестов:
```
Total tests: 8
     Passed: 8
```

Покрыты сценарии:
- ✅ Публикация одного события
- ✅ Публикация множественных событий
- ✅ Подписка на события
- ✅ Вызов хэндлеров
- ✅ Множественные хэндлеры для одного события
- ✅ Обработка ошибок в хэндлерах
