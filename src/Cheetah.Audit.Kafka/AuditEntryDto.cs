namespace Cheetah.Audit.Kafka;

/// <summary>
/// Wire-формат AuditEntry для Kafka. Изменяется отдельно от внутренней модели,
/// чтобы downstream-consumers не ломались при рефакторинге Cheetah.Audit.
/// </summary>
public sealed record AuditEntryDto(
    Guid id,
    string entityType,
    string entityId,
    string action,            // "Created" | "Updated" | "Deleted"
    string changes,           // JSON-словарь
    DateTimeOffset occurredAt,
    string? userId,
    string? userName,
    string? tenantId,
    string? correlationId);
