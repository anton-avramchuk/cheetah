using System.Diagnostics.Metrics;
using Cheetah.Core.Outbox;
using Shouldly;

namespace Cheetah.Core.Outbox.Tests;

public class OutboxMetricsTests
{
    [Fact]
    public void Все_инструменты_создаются_с_корректными_именами()
    {
        using var metrics = new OutboxMetrics();

        metrics.Published.Name.ShouldBe("outbox.published");
        metrics.Failed.Name.ShouldBe("outbox.failed");
        metrics.PublishLatencyMs.Name.ShouldBe("outbox.publish_latency");
        metrics.Cleaned.Name.ShouldBe("outbox.cleaned");
    }

    [Fact]
    public void Published_counter_регистрируется_через_MeterListener()
    {
        long collected = 0;
        using var listener = new MeterListener
        {
            InstrumentPublished = (instrument, l) =>
            {
                if (instrument.Meter.Name == OutboxMetrics.MeterName && instrument.Name == "outbox.published")
                    l.EnableMeasurementEvents(instrument);
            }
        };
        listener.SetMeasurementEventCallback<long>((_, value, _, _) => Interlocked.Add(ref collected, value));
        listener.Start();

        using var metrics = new OutboxMetrics();
        metrics.Published.Add(3);
        metrics.Published.Add(7);

        collected.ShouldBe(10);
    }
}
