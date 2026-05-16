namespace Cheetah.Core.Outbox;

/// <summary>
/// Хранилище outbox-сообщений. Реализуется на стороне конкретного DbContext.
/// </summary>
public interface IOutboxStore
{
    /// <summary>
    /// Добавить сообщение в текущую транзакцию DbContext'a.
    /// SaveChangesAsync вызывается прикладным кодом (тот же, что и для агрегата).
    /// </summary>
    ValueTask AddAsync(OutboxMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить пачку необработанных сообщений, готовых к отправке.
    /// </summary>
    ValueTask<IReadOnlyList<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Пометить сообщение как успешно обработанное.
    /// </summary>
    ValueTask MarkProcessedAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Зафиксировать ошибку и запланировать повторную попытку.
    /// </summary>
    ValueTask MarkFailedAsync(Guid id, string error, DateTimeOffset nextAttemptAt, CancellationToken cancellationToken = default);
}
