namespace Cheetah.Core.Outbox;

/// <summary>
/// Запись Inbox для идемпотентности consumer'ов:
/// перед обработкой события проверяется, что (EventId, ConsumerName) уже не присутствует.
/// </summary>
public class InboxMessage
{
    public Guid EventId { get; set; }

    public string ConsumerName { get; set; } = null!;

    public string EventType { get; set; } = null!;

    public DateTimeOffset ReceivedAt { get; set; } = DateTimeOffset.UtcNow;
}
