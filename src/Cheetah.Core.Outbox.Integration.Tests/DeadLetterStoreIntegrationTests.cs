using Cheetah.Core.Outbox;
using Cheetah.Core.Outbox.EntityFrameworkCore;
using Shouldly;

namespace Cheetah.Core.Outbox.Integration.Tests;

[Collection("Postgres")]
public class DeadLetterStoreIntegrationTests
{
    private readonly PostgresFixture _fx;

    public DeadLetterStoreIntegrationTests(PostgresFixture fx) => _fx = fx;

    [Fact]
    public async Task MoveFromOutbox_удаляет_из_outbox_и_создаёт_dead_letter_в_одной_транзакции()
    {
        await using var db = _fx.CreateDbContext();
        db.OutboxMessages.RemoveRange(db.OutboxMessages);
        db.DeadLetterMessages.RemoveRange(db.DeadLetterMessages);
        await db.SaveChangesAsync();

        var source = new OutboxMessage
        {
            EventType = "DeadEvent, Asm",
            Payload = "{\"x\":1}",
            OccurredAt = DateTimeOffset.UtcNow,
            RetryCount = 10
        };
        db.OutboxMessages.Add(source);
        await db.SaveChangesAsync();

        var dlqStore = new EfDeadLetterStore<TestDbContext>(db);
        await dlqStore.MoveFromOutboxAsync(source, "permanent failure: ...");

        db.OutboxMessages.Any(x => x.Id == source.Id).ShouldBeFalse();
        var dead = db.DeadLetterMessages.Single(x => x.Id == source.Id);
        dead.RetryCount.ShouldBe(11);
        dead.LastError.ShouldContain("permanent failure");
        dead.Payload.ShouldBe(source.Payload);
    }

    [Fact]
    public async Task Requeue_возвращает_сообщение_в_outbox_со_сброшенным_RetryCount()
    {
        await using var db = _fx.CreateDbContext();
        db.OutboxMessages.RemoveRange(db.OutboxMessages);
        db.DeadLetterMessages.RemoveRange(db.DeadLetterMessages);
        await db.SaveChangesAsync();

        var dead = new DeadLetterMessage
        {
            Id = Guid.NewGuid(),
            EventType = "X, Y",
            Payload = "{}",
            OccurredAt = DateTimeOffset.UtcNow.AddDays(-1),
            RetryCount = 15,
            LastError = "boom"
        };
        db.DeadLetterMessages.Add(dead);
        await db.SaveChangesAsync();

        var dlq = new EfDeadLetterStore<TestDbContext>(db);
        await dlq.RequeueAsync(dead.Id);

        db.DeadLetterMessages.Any(x => x.Id == dead.Id).ShouldBeFalse();
        var requeued = db.OutboxMessages.Single(x => x.Id == dead.Id);
        requeued.RetryCount.ShouldBe(0);
        requeued.ProcessedAt.ShouldBeNull();
        requeued.Error.ShouldBeNull();
    }
}
