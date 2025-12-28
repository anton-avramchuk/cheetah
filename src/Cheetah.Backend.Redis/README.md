# Cheetah.Backend.Redis

Модуль для работы с Redis с поддержкой нескольких инстансов.

## Возможности

- Поддержка нескольких инстансов Redis
- CRUD операции (Get, Set, Delete, Exists)
- Batch операции (GetMany, SetMany)
- Поиск ключей по шаблону
- Pub/Sub для событий
- Автоматическая сериализация/десериализация JSON

## Конфигурация

Добавьте в `appsettings.json`:

```json
{
  "Redis": {
    "Instances": {
      "default": {
        "ConnectionString": "localhost:6379",
        "Database": 0
      },
      "cache": {
        "ConnectionString": "localhost:6379",
        "Database": 1
      },
      "events": {
        "ConnectionString": "localhost:6380",
        "Database": 0
      }
    }
  }
}
```

## Использование

### IRedisClient - CRUD операции

```csharp
public class MyService
{
    private readonly IRedisClient _redisClient;

    public MyService(IRedisClient redisClient)
    {
        _redisClient = redisClient;
    }

    public async Task ExampleAsync()
    {
        // Сохранить значение
        await _redisClient.SetAsync("user:123", new User { Name = "John" });

        // Сохранить с TTL
        await _redisClient.SetAsync("token:abc", "value", TimeSpan.FromMinutes(5));

        // Получить значение
        var user = await _redisClient.GetAsync<User>("user:123");

        // Проверить существование
        var exists = await _redisClient.ExistsAsync("user:123");

        // Удалить
        await _redisClient.DeleteAsync("user:123");

        // Поиск ключей
        var keys = await _redisClient.SearchKeysAsync("user:*");

        // Batch операции
        var users = await _redisClient.GetManyAsync<User>(new[] { "user:1", "user:2" });

        await _redisClient.SetManyAsync(new Dictionary<string, User>
        {
            ["user:1"] = new User { Name = "Alice" },
            ["user:2"] = new User { Name = "Bob" }
        });

        // Использование другого инстанса
        await _redisClient.SetAsync("key", "value", instanceName: "cache");
    }
}
```

### IRedisEventBus - Pub/Sub

```csharp
public class MyEventService
{
    private readonly IRedisEventBus _eventBus;

    public MyEventService(IRedisEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task PublishEventAsync()
    {
        // Отправить событие
        await _eventBus.PublishAsync("user.created", new UserCreatedEvent
        {
            UserId = 123,
            Name = "John"
        });

        // Отправить в другой инстанс
        await _eventBus.PublishAsync("notifications", new Notification(), instanceName: "events");
    }

    public async Task SubscribeToEventsAsync()
    {
        // Подписаться на событие
        await _eventBus.SubscribeAsync<UserCreatedEvent>("user.created", async evt =>
        {
            Console.WriteLine($"User created: {evt.Name}");
            await Task.CompletedTask;
        });

        // Подписаться на другой инстанс
        await _eventBus.SubscribeAsync<Notification>("notifications",
            HandleNotificationAsync,
            instanceName: "events");
    }

    public async Task UnsubscribeAsync()
    {
        await _eventBus.UnsubscribeAsync("user.created");
    }

    private async Task HandleNotificationAsync(Notification notification)
    {
        // Обработка уведомления
        await Task.CompletedTask;
    }
}
```

### IRedisConnectionProvider - Низкоуровневый доступ

```csharp
public class AdvancedService
{
    private readonly IRedisConnectionProvider _connectionProvider;

    public AdvancedService(IRedisConnectionProvider connectionProvider)
    {
        _connectionProvider = connectionProvider;
    }

    public async Task AdvancedOperationsAsync()
    {
        // Получить базу данных
        var db = _connectionProvider.GetDatabase("default");

        // Использовать напрямую StackExchange.Redis API
        await db.StringSetAsync("key", "value");
        var value = await db.StringGetAsync("key");

        // Получить подключение
        var connection = _connectionProvider.GetConnection("default");
        var server = connection.GetServer(connection.GetEndPoints().First());

        // Выполнить команды сервера
        var info = await server.InfoAsync();
    }
}
```

## Регистрация

Модуль автоматически регистрирует все сервисы через атрибут `[Export]`:

- `IRedisConnectionProvider` - Singleton
- `IRedisClient` - Scoped
- `IRedisEventBus` - Singleton

## Связанные проекты

Также доступен проект **Cheetah.Backend.Events.Redis**, который предоставляет интеграцию с системой событий через `IEventBus`. См. документацию в `src/Cheetah.Backend.Events.Redis/README.md`.

## Зависимости

- `StackExchange.Redis` 2.8.16
- `Cheetah.Core`
