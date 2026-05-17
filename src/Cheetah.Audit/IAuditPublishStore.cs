namespace Cheetah.Audit;

/// <summary>
/// Хранилище, через которое publisher-сервисы читают неопубликованные AuditEntry
/// и помечают результат. Реализуется отдельным storage-модулем (EF и т.п.).
/// </summary>
public interface IAuditPublishStore
{
    /// <summary>
    /// Атомарно "захватить" пачку для публикации: вернуть pending записи и сдвинуть
    /// их NextAttemptAt вперёд, чтобы другая реплика не взяла те же.
    /// </summary>
    ValueTask<IReadOnlyList<AuditEntry>> ClaimPendingAsync(int batchSize, TimeSpan claimTimeout, CancellationToken cancellationToken = default);

    ValueTask MarkPublishedAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);

    ValueTask MarkFailedAsync(Guid id, string error, DateTimeOffset nextAttemptAt, CancellationToken cancellationToken = default);
}
