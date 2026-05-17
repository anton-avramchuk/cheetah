namespace Cheetah.Core.Outbox;

/// <summary>
/// Сообщение, исчерпавшее MaxRetries. Хранится отдельно от OutboxMessages,
/// чтобы не мешать горячему пути processor'а и быть доступным для ручного re-queue.
/// </summary>
public class DeadLetterMessage
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = null!;
    public string Payload { get; set; } = null!;
    public DateTimeOffset OccurredAt { get; set; }
    public DateTimeOffset MovedToDeadLetterAt { get; set; } = DateTimeOffset.UtcNow;
    public int RetryCount { get; set; }
    public string? LastError { get; set; }
}
