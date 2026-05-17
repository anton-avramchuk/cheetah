# Cheetah.Backend.Events.Kafka

`IEventBus` поверх Confluent.Kafka, регистрируется как **keyed-сервис** `IEventBus(EventBusKeys.Kafka)`. Дефолтную регистрацию `IEventBus` не трогает — её владельцем остаётся ваш основной транспорт (обычно `Cheetah.Backend.Events.Redis`).

## Зачем

Сценарий: приложение использует Redis pub/sub для in-process inter-module событий, но нужен дополнительный путь "publish-only в Kafka" — для аудита, аналитики, downstream-интеграций. Раньше альтернативой было держать в DI один `IEventBus`, что не давало двух разных транспортов одновременно. Теперь keyed-регистрация решает это без двусмысленности:

```csharp
public class CustomerHandler(IEventBus bus) { }  // дефолт = Redis

public class AuditPublisher(
    [FromKeyedServices(EventBusKeys.Kafka)] IEventBus kafkaBus) { }
```

## Состав

| Тип | Назначение |
|-----|------------|
| `CrmKafkaEventBus` | `IEventBus` поверх `Confluent.Kafka.IProducer<string, string>`. Subscribe бросает `NotSupportedException` — Kafka consumer-group семантика мапится плохо, для consumer'ов пишите свой `BackgroundService` |
| `KafkaEventBusOptions` | `BootstrapServers`, `TopicPrefix`, `ClientId`, `Acks`, SASL/SSL для облачных Kafka |
| `IKafkaProducerFactory` / `DefaultKafkaProducerFactory` | Создаёт producer с `EnableIdempotence=true` и настроенным `acks` |
| `CrmBackendEventsKafkaModule` | Регистрирует keyed `IEventBus(EventBusKeys.Kafka)` |

## Подключение

```csharp
[DependsOn(typeof(CrmBackendEventsRedisModule))]    // default IEventBus + keyed("redis")
[DependsOn(typeof(CrmBackendEventsKafkaModule))]    // keyed("kafka")
public partial class MyAppModule : CrmModule { }
```

```json
"KafkaEventBus": {
  "BootstrapServers": "broker1:9092,broker2:9092",
  "TopicPrefix": "events.",
  "ClientId": "myapp",
  "Acks": "all"
}
```

С такой конфигурацией событие типа `CustomerCreatedEvent`, отправленное через keyed `IEventBus(Kafka)`, попадёт в топик `events.CustomerCreatedEvent`.

### Confluent Cloud / Yandex MQ

```json
"KafkaEventBus": {
  "BootstrapServers": "pkc-xxxxx.eu-central-1.aws.confluent.cloud:9092",
  "SecurityProtocol": "SaslSsl",
  "SaslMechanism": "Plain",
  "SaslUsername": "...",
  "SaslPassword": "..."
}
```

## Wire-format

Каждое событие сериализуется в JSON. Kafka key = `EventId.ToString()` — обеспечивает дедупликацию downstream и партиционирование по сообщению. Topic = `{TopicPrefix}{typeof(TEvent).Name}` — один топик на тип события.

## Почему Subscribe не реализован

`IEventBus.Subscribe<TEvent, THandler>()` — это in-process model: handler выполняется в текущем процессе при получении события. Для Kafka же consumer требует:
- consumer group (для распределения партиций между репликами)
- offset commit policy (manual / auto / at-least-once / at-most-once)
- partition assignment + rebalancing
- consumer lifecycle (отдельный поток / hosted service)

Это слишком много решений за пользователя. Если нужен Kafka consumer — пишите явный `BackgroundService` с `IConsumer<,>`, это правильнее чем прятать его за абстракцией Subscribe.
