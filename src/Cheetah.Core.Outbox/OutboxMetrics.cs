using System.Diagnostics.Metrics;

namespace Cheetah.Core.Outbox;

/// <summary>
/// OpenTelemetry-метрики для Outbox. Регистрируется как singleton.
/// Для экспорта добавьте AddMeter(OutboxMetrics.MeterName) в OpenTelemetry-builder.
/// </summary>
public sealed class OutboxMetrics : IDisposable
{
    public const string MeterName = "Cheetah.Core.Outbox";

    private readonly Meter _meter;

    public Counter<long> Published { get; }
    public Counter<long> Failed { get; }
    public Histogram<double> PublishLatencyMs { get; }
    public Counter<long> Cleaned { get; }

    public OutboxMetrics()
    {
        _meter = new Meter(MeterName, "1.0.0");

        Published = _meter.CreateCounter<long>(
            "outbox.published",
            unit: "msg",
            description: "Сообщения, успешно опубликованные через IInnerEventBus");

        Failed = _meter.CreateCounter<long>(
            "outbox.failed",
            unit: "msg",
            description: "Сообщения, отправка которых упала и помечена для retry");

        PublishLatencyMs = _meter.CreateHistogram<double>(
            "outbox.publish_latency",
            unit: "ms",
            description: "Время от чтения сообщения из store до MarkProcessed");

        Cleaned = _meter.CreateCounter<long>(
            "outbox.cleaned",
            unit: "row",
            description: "Удалённые cleanup-сервисом строки (outbox + inbox)");
    }

    public void Dispose() => _meter.Dispose();
}
