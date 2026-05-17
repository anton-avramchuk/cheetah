using System.Diagnostics.Metrics;

namespace Cheetah.Core.Outbox.OpenTelemetry;

/// <summary>
/// Реализация IOutboxMetrics поверх <see cref="Meter"/> (System.Diagnostics.Metrics).
/// Совместима с OpenTelemetry: подключите AddMeter(<see cref="MeterName"/>) в OTel-builder.
/// </summary>
public sealed class OpenTelemetryOutboxMetrics : IOutboxMetrics, IDisposable
{
    public const string MeterName = "Cheetah.Core.Outbox";

    private readonly Meter _meter;
    private readonly Counter<long> _published;
    private readonly Counter<long> _failed;
    private readonly Histogram<double> _publishLatencyMs;
    private readonly Counter<long> _cleaned;

    public OpenTelemetryOutboxMetrics()
    {
        _meter = new Meter(MeterName, "1.0.0");

        _published = _meter.CreateCounter<long>(
            "outbox.published", unit: "msg",
            description: "Сообщения, успешно опубликованные через IInnerEventBus");

        _failed = _meter.CreateCounter<long>(
            "outbox.failed", unit: "msg",
            description: "Сообщения, отправка которых упала (включая DLQ)");

        _publishLatencyMs = _meter.CreateHistogram<double>(
            "outbox.publish_latency", unit: "ms",
            description: "Время от чтения сообщения из store до MarkProcessed");

        _cleaned = _meter.CreateCounter<long>(
            "outbox.cleaned", unit: "row",
            description: "Удалённые cleanup-сервисом строки (outbox + inbox)");
    }

    public void RecordPublished(string eventType, long count = 1)
        => _published.Add(count, new KeyValuePair<string, object?>("event_type", eventType));

    public void RecordFailed(string eventType, bool deadLetter = false)
        => _failed.Add(1,
            new KeyValuePair<string, object?>("event_type", eventType),
            new KeyValuePair<string, object?>("dead_letter", deadLetter));

    public void RecordPublishLatencyMs(string eventType, double latencyMs)
        => _publishLatencyMs.Record(latencyMs, new KeyValuePair<string, object?>("event_type", eventType));

    public void RecordCleaned(string table, long rows)
        => _cleaned.Add(rows, new KeyValuePair<string, object?>("table", table));

    public void Dispose() => _meter.Dispose();
}
