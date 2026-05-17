namespace Cheetah.Core.Outbox;

/// <summary>
/// Хранилище DLQ. Реализуется отдельно (EF Core), как и IOutboxStore.
/// </summary>
public interface IDeadLetterStore
{
    /// <summary>
    /// Атомарно: добавить запись в DLQ и удалить соответствующее OutboxMessage.
    /// </summary>
    ValueTask MoveFromOutboxAsync(OutboxMessage source, string lastError, CancellationToken cancellationToken = default);

    /// <summary>
    /// Вернуть запись из DLQ обратно в outbox (для ручного re-queue).
    /// Сбрасывает RetryCount и NextAttemptAt.
    /// </summary>
    ValueTask RequeueAsync(Guid deadLetterId, CancellationToken cancellationToken = default);
}
