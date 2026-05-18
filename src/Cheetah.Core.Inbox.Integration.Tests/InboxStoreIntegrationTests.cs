using Cheetah.Core.Inbox.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Cheetah.Core.Inbox.Integration.Tests;

[Collection("Postgres")]
public class InboxStoreIntegrationTests : IClassFixture<PostgresFixture>
{
    private readonly PostgresFixture _fx;

    public InboxStoreIntegrationTests(PostgresFixture fx) => _fx = fx;

    [Fact]
    public async Task AlreadyProcessed_Is_False_For_New_Event_And_True_After_Add()
    {
        await using var db = _fx.CreateDbContext();
        var store = new EfInboxStore<InboxTestDbContext>(db);

        var eventId = Guid.NewGuid();
        var consumer = "Test.Consumer." + Guid.NewGuid();

        (await store.AlreadyProcessedAsync(eventId, consumer)).ShouldBeFalse();

        await store.AddAsync(new InboxMessage
        {
            EventId = eventId,
            ConsumerName = consumer,
            EventType = "TestEvent",
            ReceivedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        (await store.AlreadyProcessedAsync(eventId, consumer)).ShouldBeTrue();
    }

    [Fact]
    public async Task Different_Consumers_Have_Independent_Records()
    {
        await using var db = _fx.CreateDbContext();
        var store = new EfInboxStore<InboxTestDbContext>(db);

        var eventId = Guid.NewGuid();
        var consumerA = "Consumer.A." + Guid.NewGuid();
        var consumerB = "Consumer.B." + Guid.NewGuid();

        await store.AddAsync(new InboxMessage
        {
            EventId = eventId, ConsumerName = consumerA, EventType = "T", ReceivedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        (await store.AlreadyProcessedAsync(eventId, consumerA)).ShouldBeTrue();
        (await store.AlreadyProcessedAsync(eventId, consumerB)).ShouldBeFalse();
    }

    [Fact]
    public async Task Duplicate_Insert_For_Same_Event_And_Consumer_Throws_On_PK_Violation()
    {
        await using var db1 = _fx.CreateDbContext();
        await using var db2 = _fx.CreateDbContext();

        var store1 = new EfInboxStore<InboxTestDbContext>(db1);
        var store2 = new EfInboxStore<InboxTestDbContext>(db2);

        var eventId = Guid.NewGuid();
        var consumer = "Consumer.Race." + Guid.NewGuid();

        var msg1 = new InboxMessage { EventId = eventId, ConsumerName = consumer, EventType = "T", ReceivedAt = DateTimeOffset.UtcNow };
        var msg2 = new InboxMessage { EventId = eventId, ConsumerName = consumer, EventType = "T", ReceivedAt = DateTimeOffset.UtcNow };

        await store1.AddAsync(msg1);
        await db1.SaveChangesAsync();

        await store2.AddAsync(msg2);
        await Should.ThrowAsync<DbUpdateException>(() => db2.SaveChangesAsync());
    }

    [Fact]
    public async Task DeleteOlderThan_Removes_Old_And_Keeps_Recent()
    {
        await using var db = _fx.CreateDbContext();
        var store = new EfInboxStore<InboxTestDbContext>(db);

        var old = DateTimeOffset.UtcNow.AddDays(-10);
        var recent = DateTimeOffset.UtcNow;
        var consumer = "Consumer.Cleanup." + Guid.NewGuid();

        for (var i = 0; i < 5; i++)
            db.InboxMessages.Add(new InboxMessage
            {
                EventId = Guid.NewGuid(), ConsumerName = consumer, EventType = "T", ReceivedAt = old
            });
        for (var i = 0; i < 3; i++)
            db.InboxMessages.Add(new InboxMessage
            {
                EventId = Guid.NewGuid(), ConsumerName = consumer, EventType = "T", ReceivedAt = recent
            });
        await db.SaveChangesAsync();

        var threshold = DateTimeOffset.UtcNow.AddDays(-1);

        // Drain — общий store удалит все old-записи (включая возможные из соседних тестов).
        // Проверяем не глобальный счёт, а консистентность СВОЕГО consumer'а.
        while (await store.DeleteOlderThanAsync(threshold, batchSize: 100) > 0) { }

        var ownOld = await db.InboxMessages.CountAsync(
            x => x.ConsumerName == consumer && x.ReceivedAt < threshold);
        ownOld.ShouldBe(0);

        var ownRecent = await db.InboxMessages.CountAsync(x => x.ConsumerName == consumer);
        ownRecent.ShouldBe(3);
    }

    [Fact]
    public async Task DeleteOlderThan_Respects_BatchSize()
    {
        await using var db = _fx.CreateDbContext();
        var store = new EfInboxStore<InboxTestDbContext>(db);

        var old = DateTimeOffset.UtcNow.AddDays(-10);
        var consumer = "Consumer.Batch." + Guid.NewGuid();

        for (var i = 0; i < 10; i++)
            db.InboxMessages.Add(new InboxMessage
            {
                EventId = Guid.NewGuid(), ConsumerName = consumer, EventType = "T", ReceivedAt = old
            });
        await db.SaveChangesAsync();

        var threshold = DateTimeOffset.UtcNow.AddDays(-1);

        // Каждый батч ≤ batchSize. Проверка верхней границы, а не точного равенства,
        // т.к. в общей таблице могут быть записи от других тестов.
        var firstBatch = await store.DeleteOlderThanAsync(threshold, batchSize: 3);
        firstBatch.ShouldBeLessThanOrEqualTo(3);
        firstBatch.ShouldBeGreaterThan(0);

        var secondBatch = await store.DeleteOlderThanAsync(threshold, batchSize: 3);
        secondBatch.ShouldBeLessThanOrEqualTo(3);
        secondBatch.ShouldBeGreaterThan(0);
    }
}
