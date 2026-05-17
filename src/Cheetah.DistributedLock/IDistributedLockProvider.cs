namespace Cheetah.DistributedLock;

/// <summary>
/// Источник distributed locks. Конкретные провайдеры — Postgres (pg_advisory_lock),
/// Redis (RedLock), ZooKeeper и т.п.
/// </summary>
public interface IDistributedLockProvider
{
    /// <summary>
    /// Попытаться получить блокировку без ожидания. Возвращает null если блокировка занята другим.
    /// </summary>
    ValueTask<IDistributedLock?> TryAcquireAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить блокировку, ожидая освобождения до <paramref name="timeout"/>.
    /// Бросает <see cref="DistributedLockTimeoutException"/> если за timeout не получили.
    /// </summary>
    ValueTask<IDistributedLock> AcquireAsync(string key, TimeSpan timeout, CancellationToken cancellationToken = default);
}

public sealed class DistributedLockTimeoutException : Exception
{
    public string Key { get; }
    public TimeSpan Timeout { get; }

    public DistributedLockTimeoutException(string key, TimeSpan timeout)
        : base($"Failed to acquire distributed lock '{key}' within {timeout}.")
    {
        Key = key;
        Timeout = timeout;
    }
}
