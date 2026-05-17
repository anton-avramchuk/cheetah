# Cheetah.Audit.Kafka

Доставка AuditEntries в Kafka для [Cheetah.Audit](../Cheetah.Audit/README.md). **Не работает с Kafka напрямую** — делегирует через keyed `IEventBus(EventBusKeys.Kafka)`, реализованный в [Cheetah.Backend.Events.Kafka](../Cheetah.Backend.Events.Kafka/README.md).

## Архитектура

```
SaveChanges (бизнес-транзакция)
    ↓ AuditInterceptor
    ↓ EfAuditSink → INSERT в AuditEntries (атомарно с агрегатом)
                      ▲
                      │  IAuditPublishStore.ClaimPendingAsync
KafkaAuditPublisher (BackgroundService)
    ↓ GetRequiredKeyedService<IEventBus>("kafka") → CrmKafkaEventBus
    ↓ Kafka topic "events.AuditEntryRecordedEvent" (TopicPrefix + имя события)
    ↓ MarkPublishedAsync (UPDATE PublishedAt)
```

Дефолтный `IEventBus` приложения (Redis) для in-process событий остаётся как есть — у audit свой keyed-канал к Kafka.

## Состав

| Тип | Назначение |
|-----|------------|
| `KafkaAuditPublisher` | BackgroundService: claim → publish через keyed IEventBus → mark published. Retry с экспоненциальным backoff |
| `KafkaAuditOptions` | Только publisher-цикл: `BatchSize`, `PollingInterval`, `MaxRetries`, `BaseRetryDelay`, `MaxRetryDelay`. Сама Kafka настраивается в `KafkaEventBusOptions` |
| `AuditEntryRecordedEvent` | Wire-format: стабильный контракт для downstream consumers. Шлётся через `IEventBus`, попадает в топик `{TopicPrefix}AuditEntryRecordedEvent` |
| `CrmAuditKafkaModule` | Регистрирует publisher; зависит от `CrmBackendEventsKafkaModule` |

## Подключение

```csharp
[DependsOn(typeof(CrmAuditModule))]
[DependsOn(typeof(CrmAuditEntityFrameworkCoreModule))]
[DependsOn(typeof(CrmBackendEventsKafkaModule))]   // подключаем Kafka-транспорт
[DependsOn(typeof(CrmAuditKafkaModule))]           // подключаем publisher
public partial class MyAppModule : CrmModule { }
```

```json
"KafkaEventBus": {
  "BootstrapServers": "broker1:9092,broker2:9092",
  "TopicPrefix": "events.",
  "ClientId": "myapp"
},
"Audit": {
  "Kafka": {
    "BatchSize": 100,
    "PollingInterval": "00:00:02",
    "MaxRetries": 10,
    "BaseRetryDelay": "00:00:05",
    "MaxRetryDelay": "00:10:00"
  }
}
```

SASL/SSL для облачных Kafka — в `KafkaEventBus` секции (см. Backend.Events.Kafka README).

## Wire-format (AuditEntryRecordedEvent)

Сериализуется через JSON. Kafka key = `EventId.ToString()`.

```json
{
  "eventId": "guid",
  "occurredAt": "2026-05-17T10:00:00Z",
  "auditEntryId": "guid",
  "entityType": "Crm.Customer.Domain.Customer",
  "entityId": "fc9c...",
  "action": "Updated",
  "changes": "{\"Name\":{\"old\":\"Ann\",\"new\":\"Anna\"}}",
  "userId": "u-123",
  "userName": "Anton",
  "tenantId": null,
  "correlationId": "trace-xyz"
}
```

## Multi-instance

`EfAuditPublishStore` сейчас делает claim через `ExecuteUpdate` (race-prone). Для прод-нагрузки с несколькими репликами нужен наследник с `SELECT FOR UPDATE SKIP LOCKED` (по аналогии с `Cheetah.Core.Outbox.PostgreSql/PostgresOutboxStore`).
