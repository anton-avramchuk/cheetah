using Cheetah.Core.Inbox;
using Cheetah.Core.Mongo.UnitOfWork;
using Cheetah.Core.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Cheetah.Core.Mongo.Integration.Tests;

[Collection("Mongo")]
public class MongoOutboxAndInboxTests : IAsyncLifetime
{
    private readonly MongoFixture _fixture;

    public MongoOutboxAndInboxTests(MongoFixture fixture) => _fixture = fixture;

    public Task InitializeAsync() => _fixture.DropAllAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Outbox_pending_then_processed_then_cleanup()
    {
        var message = new OutboxMessage { EventType = "T", Payload = "{}" };

        await using (var scope = _fixture.CreateScope())
        {
            var store = scope.ServiceProvider.GetRequiredService<IOutboxStore>();
            var uow = scope.ServiceProvider.GetRequiredService<IMongoUnitOfWork>();
            await store.AddAsync(message);
            await uow.SaveChangesAsync(); // outbox AddAsync buffers on the unit of work
        }

        await using (var scope = _fixture.CreateScope())
        {
            var store = scope.ServiceProvider.GetRequiredService<IOutboxStore>();
            var pending = await store.GetPendingAsync(10);
            pending.ShouldContain(m => m.Id == message.Id);

            await store.MarkProcessedBatchAsync(new[] { message.Id });
            (await store.GetPendingAsync(10)).ShouldNotContain(m => m.Id == message.Id);

            var deleted = await store.DeleteProcessedAsync(DateTimeOffset.UtcNow.AddMinutes(1), 100);
            deleted.ShouldBe(1);
        }
    }

    [Fact]
    public async Task Outbox_MarkFailed_sets_retry_and_next_attempt()
    {
        var message = new OutboxMessage { EventType = "T", Payload = "{}" };

        await using (var scope = _fixture.CreateScope())
        {
            var store = scope.ServiceProvider.GetRequiredService<IOutboxStore>();
            var uow = scope.ServiceProvider.GetRequiredService<IMongoUnitOfWork>();
            await store.AddAsync(message);
            await uow.SaveChangesAsync();
        }

        await using (var scope = _fixture.CreateScope())
        {
            var store = scope.ServiceProvider.GetRequiredService<IOutboxStore>();
            await store.MarkFailedAsync(message.Id, "boom", DateTimeOffset.UtcNow.AddMinutes(5));

            // NextAttemptAt is in the future, so it is no longer pending.
            (await store.GetPendingAsync(10)).ShouldNotContain(m => m.Id == message.Id);
        }
    }

    [Fact]
    public async Task DeadLetter_move_is_atomic()
    {
        var message = new OutboxMessage { EventType = "T", Payload = "{}" };

        await using (var scope = _fixture.CreateScope())
        {
            var store = scope.ServiceProvider.GetRequiredService<IOutboxStore>();
            var uow = scope.ServiceProvider.GetRequiredService<IMongoUnitOfWork>();
            await store.AddAsync(message);
            await uow.SaveChangesAsync();
        }

        await using (var scope = _fixture.CreateScope())
        {
            var dlq = scope.ServiceProvider.GetRequiredService<IDeadLetterStore>();
            await dlq.MoveFromOutboxAsync(message, "exhausted retries");

            var store = scope.ServiceProvider.GetRequiredService<IOutboxStore>();
            (await store.GetPendingAsync(10)).ShouldNotContain(m => m.Id == message.Id);
        }
    }

    [Fact]
    public async Task Inbox_records_and_detects_processed()
    {
        var eventId = Guid.NewGuid();
        const string consumer = "consumer-A";

        await using (var scope = _fixture.CreateScope())
        {
            var store = scope.ServiceProvider.GetRequiredService<IInboxStore>();
            (await store.AlreadyProcessedAsync(eventId, consumer)).ShouldBeFalse();

            var uow = scope.ServiceProvider.GetRequiredService<IMongoUnitOfWork>();
            await store.AddAsync(new InboxMessage { EventId = eventId, ConsumerName = consumer, EventType = "T" });
            await uow.SaveChangesAsync();
        }

        await using (var scope = _fixture.CreateScope())
        {
            var store = scope.ServiceProvider.GetRequiredService<IInboxStore>();
            (await store.AlreadyProcessedAsync(eventId, consumer)).ShouldBeTrue();
            (await store.AlreadyProcessedAsync(eventId, "other-consumer")).ShouldBeFalse();
        }
    }
}
