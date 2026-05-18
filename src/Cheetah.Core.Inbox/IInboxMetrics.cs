namespace Cheetah.Core.Inbox;

/// <summary>
/// Точки метрик Inbox. Реализация по умолчанию — null-провайдер; OpenTelemetry-обёртка подключается отдельно.
/// </summary>
public interface IInboxMetrics
{
    /// <summary>Запись о том, что событие было пропущено как уже обработанное.</summary>
    void RecordDuplicateSkipped(string consumerName);

    /// <summary>Запись об успешной фиксации обработки.</summary>
    void RecordProcessed(string consumerName);

    /// <summary>Сколько строк удалил cleanup за итерацию.</summary>
    void RecordCleaned(int count);
}

internal sealed class NullInboxMetrics : IInboxMetrics
{
    public static readonly NullInboxMetrics Instance = new();
    public void RecordDuplicateSkipped(string consumerName) { }
    public void RecordProcessed(string consumerName) { }
    public void RecordCleaned(int count) { }
}
