namespace Cheetah.Saga;

public interface ISagaRepository
{
    /// <summary>
    /// Найти существующую saga instance по (sagaType, correlationKey).
    /// </summary>
    ValueTask<SagaInstance?> FindAsync(string sagaType, string correlationKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Добавить новую saga instance.
    /// </summary>
    ValueTask AddAsync(SagaInstance instance, CancellationToken cancellationToken = default);

    /// <summary>
    /// Сохранить изменения. Реализация должна использовать оптимистичную блокировку
    /// (поле Version) и бросить SagaConcurrencyException при конфликте.
    /// </summary>
    ValueTask SaveChangesAsync(CancellationToken cancellationToken = default);
}

public sealed class SagaConcurrencyException : Exception
{
    public SagaConcurrencyException(string sagaType, string correlationKey)
        : base($"Concurrent update on saga {sagaType}/{correlationKey}")
    {
        SagaType = sagaType;
        CorrelationKey = correlationKey;
    }

    public string SagaType { get; }
    public string CorrelationKey { get; }
}
