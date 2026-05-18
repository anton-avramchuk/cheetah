using Cheetah.Core.Events;
using Cheetah.Core.Inbox.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace Cheetah.Core.Inbox.Integration.Tests;

/// <summary>
/// End-to-end: повторная доставка одного события через InboxIdempotentEventHandler
/// + EfInboxStore приводит ровно к одному вызову inner-handler'а.
/// </summary>
[Collection("Postgres")]
public class InboxIdempotencyIntegrationTests : IClassFixture<PostgresFixture>
{
    private readonly PostgresFixture _fx;

    public InboxIdempotencyIntegrationTests(PostgresFixture fx) => _fx = fx;

    [Fact]
    public async Task Duplicate_Event_Delivered_Twice_Is_Processed_Once()
    {
        var inner = new CountingHandler();
        var @event = new SampleEvent("payload");

        // Первая доставка
        await using (var db1 = _fx.CreateDbContext())
        {
            var store = new EfInboxStore<InboxTestDbContext>(db1);
            var decorator = new InboxIdempotentEventHandler<SampleEvent>(
                inner, store, NullLogger<InboxIdempotentEventHandler<SampleEvent>>.Instance);

            await decorator.HandleAsync(@event);
            await db1.SaveChangesAsync();
        }

        // Вторая доставка того же события — в новом scope (как при at-least-once Kafka)
        await using (var db2 = _fx.CreateDbContext())
        {
            var store = new EfInboxStore<InboxTestDbContext>(db2);
            var decorator = new InboxIdempotentEventHandler<SampleEvent>(
                inner, store, NullLogger<InboxIdempotentEventHandler<SampleEvent>>.Instance);

            await decorator.HandleAsync(@event);
            await db2.SaveChangesAsync();
        }

        inner.CallCount.ShouldBe(1);

        // Проверка, что в БД ровно одна запись для этого (EventId, ConsumerName)
        await using var verify = _fx.CreateDbContext();
        var count = await verify.InboxMessages
            .CountAsync(x => x.EventId == @event.EventId && x.ConsumerName == typeof(CountingHandler).FullName);
        count.ShouldBe(1);
    }

    public record SampleEvent(string Payload) : EventBase;

    private sealed class CountingHandler : IEventHandler<SampleEvent>
    {
        public int CallCount { get; private set; }
        public ValueTask HandleAsync(SampleEvent @event, CancellationToken cancellationToken = default)
        {
            CallCount++;
            return ValueTask.CompletedTask;
        }
    }
}
