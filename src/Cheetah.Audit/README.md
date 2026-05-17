# Cheetah.Audit

Core-абстракции системы аудита: хранение истории изменений сущностей с диффами, PII-маскированием и контекстом пользователя.

Хранилища и доставщики подключаются отдельными модулями:
- [Cheetah.Audit.EntityFrameworkCore](../Cheetah.Audit.EntityFrameworkCore/README.md) — запись в БД (канонический store)
- [Cheetah.Audit.Kafka](../Cheetah.Audit.Kafka/README.md) — фоновая публикация в Kafka topic

## Состав

| Тип | Назначение |
|-----|------------|
| `AuditEntry` | Запись лога: EntityType, EntityId, Action, Changes (JSON), OccurredAt, UserId/Name/Tenant/Correlation, PublishedAt + retry-поля |
| `[Auditable]` | На класс entity — включает аудит для этого типа |
| `[NotAudited]` | На property — исключить из diff (например, LoginCount) |
| `[Sensitive]` | На property — значения маскируются как `***` (для PII: PasswordHash, токены) |
| `IAuditUserAccessor` | Кто действует — UserId/UserName/TenantId/CorrelationId. Прикладной код регистрирует свою реализацию поверх IHttpContextAccessor |
| `IAuditSink` | Куда улетают entries из interceptor'a. EfAuditSink, KafkaAuditSink, ... |
| `IAuditPublishStore` | Для downstream-publisher'ов: claim pending → mark published/failed |
| `AuditInterceptor` | EF Core SaveChangesInterceptor: собирает Auditable-сущности из ChangeTracker, строит diff, отдаёт sinks |
| `AuditChangesBuilder` | Строит JSON-diff `{property: {old, new}}` уважая NotAudited/Sensitive |
| `CrmAuditModule` | Регистрирует interceptor и NullAuditUserAccessor по умолчанию |

## Использование

```csharp
[Auditable]
public class Customer
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? Email { get; set; }

    [Sensitive]
    public string? PasswordHash { get; set; }

    [NotAudited]
    public int LoginCount { get; set; }
}
```

Каждый `SaveChanges` который меняет Customer создаст соответствующий `AuditEntry` в той же транзакции что и сам Customer (атомарность). Если транзакция откатывается — audit тоже.

## Атомарность

`AuditInterceptor` хукает `SavingChangesAsync` — до фактического commit'а. `EfAuditSink` добавляет `AuditEntry` в тот же ChangeTracker. EF сохраняет всё одним батчем под одной транзакцией. Это значит:

- **Бизнес-операция упала** → audit тоже не сохранится. Никаких phantom-записей.
- **Audit-запись упала** (например, отвал БД) → бизнес-операция тоже откатится. Compliance-критично: не существует "сохранили клиента, но не залогировали изменение".

## Доставка в Kafka

См. [Cheetah.Audit.Kafka](../Cheetah.Audit.Kafka/README.md). Publisher работает отдельно: читает unpublished AuditEntries из БД и шлёт в топик. Это **не использует** `IEventBus` приложения — у audit'а свой топик, своя retention-политика, свои consumers.
