namespace Cheetah.Core.Outbox;

/// <summary>
/// Запись очереди исходящих событий. Сохраняется в одной транзакции с агрегатом,
/// затем публикуется фоновым процессором.
/// </summary>
public class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// AssemblyQualifiedName типа события — нужен для десериализации.
    /// </summary>
    public string EventType { get; set; } = null!;

    /// <summary>
    /// JSON-payload события.
    /// </summary>
    public string Payload { get; set; } = null!;

    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? ProcessedAt { get; set; }

    public int RetryCount { get; set; }

    public DateTimeOffset? NextAttemptAt { get; set; }

    public string? Error { get; set; }
}
