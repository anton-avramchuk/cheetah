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
    /// Пометить пачку сообщений как обработанные одним UPDATE'ом.
    /// Под нагрузкой это драматически снижает время фиксации batch'а.
    /// </summary>
    ValueTask MarkProcessedBatchAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// Зафиксировать ошибку и запланировать повторную попытку.
    /// </summary>
    ValueTask MarkFailedAsync(Guid id, string error, DateTimeOffset nextAttemptAt, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удалить обработанные сообщения, которые старше olderThan. Возвращает число удалённых.
    /// Реализация должна работать batch'ами не более <paramref name="batchSize"/>.
    /// </summary>
    ValueTask<int> DeleteProcessedAsync(DateTimeOffset olderThan, int batchSize, CancellationToken cancellationToken = default);
}
