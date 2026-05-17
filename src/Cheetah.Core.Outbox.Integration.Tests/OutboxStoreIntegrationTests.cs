using Cheetah.Core.Outbox;
using Cheetah.Core.Outbox.EntityFrameworkCore;
using Cheetah.Core.Outbox.PostgreSql;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Cheetah.Core.Outbox.Integration.Tests;

[Collection("Postgres")]
public class OutboxStoreIntegrationTests : IClassFixture<PostgresFixture>
{
    private readonly PostgresFixture _fx;

    public OutboxStoreIntegrationTests(PostgresFixture fx) => _fx = fx;

    [Fact]
    public async Task EfOutboxStore_сохраняет_и_возвращает_pending()
    {
        await using var db = _fx.CreateDbContext();
        var store = new EfOutboxStore<TestDbContext>(db);

        var message = new OutboxMessage
        {
            EventType = "TestEvent, TestAsm",
            Payload = "{}",
            OccurredAt = DateTimeOffset.UtcNow
        };
        await store.AddAsync(message);
        await db.SaveChangesAsync();

        var pending = await store.GetPendingAsync(10);
        pending.ShouldContain(m => m.Id == message.Id);

        await store.MarkProcessedAsync(message.Id);

        var pendingAfter = await store.GetPendingAsync(10);
        pendingAfter.ShouldNotContain(m => m.Id == message.Id);
    }

    [Fact]
    public async Task PostgresOutboxStore_SKIP_LOCKED_не_отдаёт_одно_сообщение_двум_репликам()
    {
        // Чистим таблицу от предыдущих тестов
        await using (var cleanup = _fx.CreateDbContext())
        {
            cleanup.OutboxMessages.RemoveRange(cleanup.OutboxMessages);
            await cleanup.SaveChangesAsync();
        }

        // Создаём 10 сообщений
        await using (var seed = _fx.CreateDbContext())
        {
            for (var i = 0; i < 10; i++)
            {
                seed.OutboxMessages.Add(new OutboxMessage
                {
                    EventType = $"E{i}, TestAsm",
                    Payload = "{}",
                    OccurredAt = DateTimeOffset.UtcNow.AddSeconds(-i)
                });
            }
            await seed.SaveChangesAsync();
        }

        var options = Microsoft.Extensions.Options.Options.Create(new PostgresOutboxOptions { ClaimTimeout = TimeSpan.FromMinutes(5) });

        // Две "реплики" — два независимых store с разными DbContext'ами
        await using var db1 = _fx.CreateDbContext();
        await using var db2 = _fx.CreateDbContext();
        var store1 = new PostgresOutboxStore<TestDbContext>(db1, options);
        var store2 = new PostgresOutboxStore<TestDbContext>(db2, options);

        // Параллельный claim
        var batch1Task = store1.GetPendingAsync(10).AsTask();
        var batch2Task = store2.GetPendingAsync(10).AsTask();
        await Task.WhenAll(batch1Task, batch2Task);
        var batch1 = await batch1Task;
        var batch2 = await batch2Task;

        var ids1 = batch1.Select(m => m.Id).ToHashSet();
        var ids2 = batch2.Select(m => m.Id).ToHashSet();
        var overlap = ids1.Intersect(ids2).ToList();

        overlap.ShouldBeEmpty("два claim'а не должны пересекаться");
        (batch1.Count + batch2.Count).ShouldBe(10);
    }

    [Fact]
    public async Task MarkProcessedBatchAsync_обновляет_все_id_одним_UPDATE_ом()
    {
        await using var db = _fx.CreateDbContext();
        db.OutboxMessages.RemoveRange(db.OutboxMessages);
        await db.SaveChangesAsync();

        var msgs = Enumerable.Range(0, 20).Select(i => new OutboxMessage
        {
            EventType = "BatchE, A", Payload = "{}",
            OccurredAt = DateTimeOffset.UtcNow.AddSeconds(-i)
        }).ToList();
        db.OutboxMessages.AddRange(msgs);
        await db.SaveChangesAsync();

        var store = new EfOutboxStore<TestDbContext>(db);
        var ids = msgs.Select(m => m.Id).ToArray();
        await store.MarkProcessedBatchAsync(ids);

        await using var verify = _fx.CreateDbContext();
        var processed = verify.OutboxMessages.Where(x => ids.Contains(x.Id) && x.ProcessedAt != null).Count();
        processed.ShouldBe(20);
    }

    [Fact]
    public async Task DeleteProcessedAsync_удаляет_только_старые_обработанные()
    {
        await using var db = _fx.CreateDbContext();
        db.OutboxMessages.RemoveRange(db.OutboxMessages);
        await db.SaveChangesAsync();

        var oldProcessed = new OutboxMessage
        {
            EventType = "Old, A", Payload = "{}",
            OccurredAt = DateTimeOffset.UtcNow.AddDays(-30),
            ProcessedAt = DateTimeOffset.UtcNow.AddDays(-30)
        };
        var recentProcessed = new OutboxMessage
        {
            EventType = "Recent, A", Payload = "{}",
            OccurredAt = DateTimeOffset.UtcNow,
            ProcessedAt = DateTimeOffset.UtcNow
        };
        var pending = new OutboxMessage
        {
            EventType = "Pending, A", Payload = "{}",
            OccurredAt = DateTimeOffset.UtcNow.AddDays(-30)
            // ProcessedAt == null
        };
        db.OutboxMessages.AddRange(oldProcessed, recentProcessed, pending);
        await db.SaveChangesAsync();

        var store = new EfOutboxStore<TestDbContext>(db);
        var threshold = DateTimeOffset.UtcNow.AddDays(-7);
        var deleted = await store.DeleteProcessedAsync(threshold, 100);

        deleted.ShouldBe(1);

        var remaining = db.OutboxMessages.Select(x => x.EventType).ToList();
        remaining.ShouldContain("Recent, A");
        remaining.ShouldContain("Pending, A");
        remaining.ShouldNotContain("Old, A");
    }
}

[CollectionDefinition("Postgres")]
public class PostgresCollection : ICollectionFixture<PostgresFixture> { }
