using Cheetah.Audit;
using Cheetah.Audit.Kafka;
using Cheetah.Core.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;

namespace Cheetah.Audit.Tests;

public class KafkaAuditPublisherTests
{
    [Fact]
    public async Task Publisher_Publishes_Pending_Via_Keyed_IEventBus_And_Marks_Published()
    {
        var pending = new[] { Sample("E1"), Sample("E2") };

        var store = new Mock<IAuditPublishStore>();
        var first = true;
        store.Setup(s => s.ClaimPendingAsync(It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(() => first
                ? new ValueTask<IReadOnlyList<AuditEntry>>(pending)
                : new ValueTask<IReadOnlyList<AuditEntry>>(Array.Empty<AuditEntry>()))
            .Callback(() => first = false);
        store.Setup(s => s.MarkPublishedAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        var bus = new Mock<IEventBus>();
        var published = new List<AuditEntryRecordedEvent>();
        bus.Setup(b => b.PublishAsync(It.IsAny<AuditEntryRecordedEvent>(), It.IsAny<CancellationToken>()))
            .Returns<AuditEntryRecordedEvent, CancellationToken>((e, _) =>
            {
                published.Add(e);
                return ValueTask.CompletedTask;
            });

        var sut = BuildPublisher(store.Object, bus.Object);
        await RunOnce(sut);

        published.Count.ShouldBe(2);
        published.ShouldContain(e => e.Changes == "E1");
        published.ShouldContain(e => e.Changes == "E2");
        store.Verify(s => s.MarkPublishedAsync(
            It.Is<IReadOnlyCollection<Guid>>(ids => ids.Count == 2 && ids.Contains(pending[0].Id) && ids.Contains(pending[1].Id)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Publisher_Calls_MarkFailed_On_IEventBus_Exception()
    {
        var entry = Sample("FAIL");
        var store = new Mock<IAuditPublishStore>();
        var first = true;
        store.Setup(s => s.ClaimPendingAsync(It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(() => first
                ? new ValueTask<IReadOnlyList<AuditEntry>>(new[] { entry })
                : new ValueTask<IReadOnlyList<AuditEntry>>(Array.Empty<AuditEntry>()))
            .Callback(() => first = false);
        store.Setup(s => s.MarkFailedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        var bus = new Mock<IEventBus>();
        bus.Setup(b => b.PublishAsync(It.IsAny<AuditEntryRecordedEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        var sut = BuildPublisher(store.Object, bus.Object);
        await RunOnce(sut);

        store.Verify(s => s.MarkFailedAsync(entry.Id,
            It.Is<string>(e => e.Contains("boom")),
            It.IsAny<DateTimeOffset>(),
            It.IsAny<CancellationToken>()), Times.Once);
        store.Verify(s => s.MarkPublishedAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AuditEntryRecordedEvent_Contains_All_AuditEntry_Fields()
    {
        var entry = new AuditEntry
        {
            EntityType = "My.E", EntityId = "id-1",
            Action = AuditAction.Updated, Changes = "{\"x\":1}",
            OccurredAt = DateTimeOffset.UtcNow,
            UserId = "u", UserName = "U", TenantId = "t", CorrelationId = "c"
        };
        var store = StoreOnce(entry);

        AuditEntryRecordedEvent? captured = null;
        var bus = new Mock<IEventBus>();
        bus.Setup(b => b.PublishAsync(It.IsAny<AuditEntryRecordedEvent>(), It.IsAny<CancellationToken>()))
            .Returns<AuditEntryRecordedEvent, CancellationToken>((e, _) => { captured = e; return ValueTask.CompletedTask; });

        await RunOnce(BuildPublisher(store, bus.Object));

        captured.ShouldNotBeNull();
        captured!.AuditEntryId.ShouldBe(entry.Id);
        captured.EntityType.ShouldBe("My.E");
        captured.EntityId.ShouldBe("id-1");
        captured.Action.ShouldBe("Updated");
        captured.UserId.ShouldBe("u");
        captured.TenantId.ShouldBe("t");
    }

    // ---------- helpers ----------

    private static AuditEntry Sample(string changesPayload = "{}", string entityId = "id-1", string entityType = "Sample.Entity") => new()
    {
        Id = Guid.NewGuid(),
        EntityType = entityType,
        EntityId = entityId,
        Action = AuditAction.Created,
        Changes = changesPayload,
        OccurredAt = DateTimeOffset.UtcNow
    };

    private static IAuditPublishStore StoreOnce(AuditEntry entry)
    {
        var m = new Mock<IAuditPublishStore>();
        var first = true;
        m.Setup(s => s.ClaimPendingAsync(It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(() => first
                ? new ValueTask<IReadOnlyList<AuditEntry>>(new[] { entry })
                : new ValueTask<IReadOnlyList<AuditEntry>>(Array.Empty<AuditEntry>()))
            .Callback(() => first = false);
        m.Setup(s => s.MarkPublishedAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);
        return m.Object;
    }

    private static KafkaAuditPublisher BuildPublisher(IAuditPublishStore store, IEventBus kafkaBus)
    {
        var services = new ServiceCollection();
        services.AddSingleton(store);
        services.AddKeyedSingleton(EventBusKeys.Kafka, (sp, _) => kafkaBus);
        var sp = services.BuildServiceProvider();

        return new KafkaAuditPublisher(
            sp,
            Microsoft.Extensions.Options.Options.Create(new KafkaAuditOptions
            {
                PollingInterval = TimeSpan.FromMilliseconds(100)
            }),
            NullLogger<KafkaAuditPublisher>.Instance);
    }

    private static async Task RunOnce(KafkaAuditPublisher pub)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(800));
        await pub.StartAsync(cts.Token);
        await Task.Delay(400);
        await pub.StopAsync(CancellationToken.None);
    }
}
