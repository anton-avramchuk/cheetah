namespace Cheetah.Core.Outbox;

/// <summary>
/// Абстракция метрик Outbox. По умолчанию подключён <see cref="NullOutboxMetrics"/> (no-op).
/// Для реального экспорта подключите модуль <c>Cheetah.Core.Outbox.OpenTelemetry</c>,
/// который заменит регистрацию на реализацию поверх <see cref="System.Diagnostics.Metrics.Meter"/>.
/// </summary>
public interface IOutboxMetrics
{
    void RecordPublished(string eventType, long count = 1);

    void RecordFailed(string eventType, bool deadLetter = false);

    void RecordPublishLatencyMs(string eventType, double latencyMs);

    void RecordCleaned(string table, long rows);
}

internal sealed class NullOutboxMetrics : IOutboxMetrics
{
    public static readonly NullOutboxMetrics Instance = new();

    public void RecordPublished(string eventType, long count = 1) { }
    public void RecordFailed(string eventType, bool deadLetter = false) { }
    public void RecordPublishLatencyMs(string eventType, double latencyMs) { }
    public void RecordCleaned(string table, long rows) { }
}
