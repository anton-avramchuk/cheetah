namespace Cheetah.Core.Inbox;

/// <summary>
/// Хранилище inbox-записей для идемпотентной обработки входящих событий.
/// </summary>
public interface IInboxStore
{
    /// <summary>
    /// Проверяет, было ли уже обработано (EventId, ConsumerName).
    /// </summary>
    ValueTask<bool> AlreadyProcessedAsync(Guid eventId, string consumerName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Фиксирует запись об обработке. SaveChangesAsync вызывает прикладной код.
    /// </summary>
    ValueTask AddAsync(InboxMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удалить inbox-записи старше olderThan. Возвращает число удалённых.
    /// </summary>
    ValueTask<int> DeleteOlderThanAsync(DateTimeOffset olderThan, int batchSize, CancellationToken cancellationToken = default);
}
