using Cheetah.Core.Events;

namespace Cheetah.Audit.Kafka;

/// <summary>
/// Wire-формат AuditEntry для отправки через IEventBus в Kafka. Стабильный контракт для
/// downstream consumers — НЕ менять при рефакторинге внутренней модели AuditEntry.
///
/// Topic: "events.AuditEntryRecordedEvent" (из CrmKafkaEventBus.TopicPrefix + имени типа).
/// </summary>
public sealed record AuditEntryRecordedEvent : EventBase
{
    public required Guid AuditEntryId { get; init; }
    public required string EntityType { get; init; }
    public required string EntityId { get; init; }
    public required string Action { get; init; }       // "Created" | "Updated" | "Deleted"
    public required string Changes { get; init; }      // JSON

    public string? UserId { get; init; }
    public string? UserName { get; init; }
    public string? TenantId { get; init; }
    public string? CorrelationId { get; init; }
}
