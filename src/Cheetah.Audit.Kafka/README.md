# Cheetah.Audit.Kafka

Доставка AuditEntries в Kafka topic для [Cheetah.Audit](../Cheetah.Audit/README.md).

## Архитектура

```
SaveChanges (бизнес-транзакция)
    ↓ AuditInterceptor
    ↓ EfAuditSink → INSERT в AuditEntries (атомарно с агрегатом)
                      ▲
                      │  IAuditPublishStore.ClaimPendingAsync (SKIP LOCKED via Postgres)
KafkaAuditPublisher (BackgroundService)
    ↓ Confluent.Kafka producer (idempotent, acks=all)
    ↓ Kafka topic "audit.events"
    ↓ MarkPublishedAsync (UPDATE PublishedAt)
```

**НЕ использует** `IEventBus` приложения — Audit получает свою независимую доставку через выделенный publisher. Это значит:
- Можно держать Redis для in-process событий приложения и Kafka только для audit.
- Audit-topic имеет свою retention (compliance: годы), independent от event bus конфигурации.
- Никаких "двух IEventBus" в DI — два разных абстракции для двух разных задач.

## Состав

| Тип | Назначение |
|-----|------------|
| `KafkaAuditPublisher` | BackgroundService: claim → produce → mark published. Retry с экспоненциальным backoff |
| `KafkaAuditOptions` | BootstrapServers, Topic, ClientId, Acks, BatchSize, retry, SASL/SSL для облачных Kafka |
| `IKafkaProducerFactory` / `DefaultKafkaProducerFactory` | Создаёт `IProducer<string,string>` с idempotence + acks=all. Подменяется в тестах |
| `AuditEntryDto` | Wire-формат для Kafka (отдельно от внутренней модели — стабильный контракт для downstream) |
| `CrmAuditKafkaModule` | Регистрирует publisher + factory |

## Подключение

```csharp
[DependsOn(typeof(CrmAuditModule))]
[DependsOn(typeof(CrmAuditEntityFrameworkCoreModule))]
[DependsOn(typeof(CrmAuditKafkaModule))]
public partial class MyAppModule : CrmModule { }
```

```json
"Audit": {
  "Kafka": {
    "BootstrapServers": "broker1:9092,broker2:9092",
    "Topic": "audit.events",
    "ClientId": "myapp-audit",
    "Acks": "all",
    "BatchSize": 100,
    "PollingInterval": "00:00:02",
    "MaxRetries": 10,
    "BaseRetryDelay": "00:00:05",
    "MaxRetryDelay": "00:10:00"
  }
}
```

### Confluent Cloud / Yandex Message Queue

```json
"Audit": {
  "Kafka": {
    "BootstrapServers": "pkc-xxxxx.eu-central-1.aws.confluent.cloud:9092",
    "SecurityProtocol": "SaslSsl",
    "SaslMechanism": "Plain",
    "SaslUsername": "...",
    "SaslPassword": "..."
  }
}
```

## Wire-формат

```json
{
  "id": "guid",
  "entityType": "Crm.Customer.Domain.Customer",
  "entityId": "fc9c...",
  "action": "Updated",
  "changes": "{\"Name\":{\"old\":\"Ann\",\"new\":\"Anna\"}}",
  "occurredAt": "2026-05-17T10:00:00Z",
  "userId": "u-123",
  "userName": "Anton",
  "tenantId": null,
  "correlationId": "trace-xyz"
}
```

Kafka key = `{entityType}:{entityId}` — обеспечивает per-aggregate ordering и партицирование по сущности.

## Multi-instance

`EfAuditPublishStore` сейчас делает claim через `ExecuteUpdate` (race-prone между репликами). Для прод-нагрузки с несколькими репликами рекомендуется наследник с `SELECT FOR UPDATE SKIP LOCKED` (по аналогии с `Cheetah.Core.Outbox.PostgreSql/PostgresOutboxStore`).
