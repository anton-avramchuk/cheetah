namespace Cheetah.Audit;

/// <summary>
/// Запись audit-лога. Хранится в БД; Kafka-публикатор отправляет её во внешний топик.
/// </summary>
public class AuditEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>FullName типа сущности (например "Crm.Customer.Domain.Customer").</summary>
    public string EntityType { get; set; } = null!;

    /// <summary>Строковое представление PK (Guid.ToString() / int.ToString()).</summary>
    public string EntityId { get; set; } = null!;

    public AuditAction Action { get; set; }

    /// <summary>JSON-словарь {property: {old, new}}. См. AuditChangesBuilder.</summary>
    public string Changes { get; set; } = "{}";

    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;

    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? TenantId { get; set; }
    public string? CorrelationId { get; set; }

    // Поля для Kafka-публикатора:
    public DateTimeOffset? PublishedAt { get; set; }
    public int RetryCount { get; set; }
    public DateTimeOffset? NextAttemptAt { get; set; }
    public string? Error { get; set; }
}
