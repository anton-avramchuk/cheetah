using System.Diagnostics.Metrics;
using Cheetah.Core.Outbox.OpenTelemetry;
using Shouldly;

namespace Cheetah.Core.Outbox.OpenTelemetry.Tests;

public class OpenTelemetryOutboxMetricsTests
{
    [Fact]
    public void RecordPublished_отдаёт_события_в_MeterListener_с_тегом_event_type()
    {
        long total = 0;
        string? lastTag = null;
        using var listener = new MeterListener
        {
            InstrumentPublished = (instrument, l) =>
            {
                if (instrument.Meter.Name == OpenTelemetryOutboxMetrics.MeterName && instrument.Name == "outbox.published")
                    l.EnableMeasurementEvents(instrument);
            }
        };
        listener.SetMeasurementEventCallback<long>((_, value, tags, _) =>
        {
            Interlocked.Add(ref total, value);
            foreach (var t in tags)
                if (t.Key == "event_type") lastTag = t.Value?.ToString();
        });
        listener.Start();

        using var metrics = new OpenTelemetryOutboxMetrics();
        metrics.RecordPublished("MyEvent", 5);

        total.ShouldBe(5);
        lastTag.ShouldBe("MyEvent");
    }

    [Fact]
    public void RecordFailed_проставляет_dead_letter_tag()
    {
        bool? deadLetter = null;
        using var listener = new MeterListener
        {
            InstrumentPublished = (i, l) =>
            {
                if (i.Meter.Name == OpenTelemetryOutboxMetrics.MeterName && i.Name == "outbox.failed")
                    l.EnableMeasurementEvents(i);
            }
        };
        listener.SetMeasurementEventCallback<long>((_, _, tags, _) =>
        {
            foreach (var t in tags)
                if (t.Key == "dead_letter") deadLetter = (bool?)t.Value;
        });
        listener.Start();

        using var metrics = new OpenTelemetryOutboxMetrics();
        metrics.RecordFailed("X", deadLetter: true);

        deadLetter.ShouldBe(true);
    }
}
