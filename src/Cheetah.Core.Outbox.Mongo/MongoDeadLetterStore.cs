using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Mongo.Connections;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Cheetah.Core.Outbox.Mongo;

/// <summary>
/// MongoDB implementation of <see cref="IDeadLetterStore"/>. <see cref="MoveFromOutboxAsync"/> and
/// <see cref="RequeueAsync"/> run inside their own session-backed transaction so the insert/delete
/// pair is atomic (requires a replica set).
/// </summary>
[Export(LifetimeType.Scoped, typeof(IDeadLetterStore))]
public class MongoDeadLetterStore : IDeadLetterStore
{
    private readonly IMongoDatabaseProvider _databaseProvider;
    private readonly MongoOutboxStoreOptions _options;

    /// <summary>Initializes a new instance of the <see cref="MongoDeadLetterStore"/> class.</summary>
    public MongoDeadLetterStore(
        IMongoDatabaseProvider databaseProvider,
        IOptions<MongoOutboxStoreOptions> options)
    {
        _databaseProvider = databaseProvider;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async ValueTask MoveFromOutboxAsync(OutboxMessage source, string lastError, CancellationToken cancellationToken = default)
    {
        var dead = new DeadLetterMessage
        {
            Id = source.Id,
            EventType = source.EventType,
            Payload = source.Payload,
            OccurredAt = source.OccurredAt,
            RetryCount = source.RetryCount + 1,
            LastError = Truncate(lastError, 4000),
            MovedToDeadLetterAt = DateTimeOffset.UtcNow,
        };

        await InTransactionAsync(async (db, session, ct) =>
        {
            await db.GetCollection<DeadLetterMessage>(_options.DeadLetterCollection)
                .InsertOneAsync(session, dead, cancellationToken: ct);
            await db.GetCollection<OutboxMessage>(_options.OutboxCollection)
                .DeleteOneAsync(session, x => x.Id == source.Id, cancellationToken: ct);
        }, cancellationToken);
    }

    /// <inheritdoc />
    public async ValueTask RequeueAsync(Guid deadLetterId, CancellationToken cancellationToken = default)
    {
        var database = await _databaseProvider.GetDatabaseAsync(_options.ConnectionName, cancellationToken);
        var deadLetters = database.GetCollection<DeadLetterMessage>(_options.DeadLetterCollection);

        var dead = await deadLetters.Find(x => x.Id == deadLetterId).FirstOrDefaultAsync(cancellationToken);
        if (dead is null)
            return;

        var requeued = new OutboxMessage
        {
            Id = dead.Id,
            EventType = dead.EventType,
            Payload = dead.Payload,
            OccurredAt = dead.OccurredAt,
            RetryCount = 0,
            NextAttemptAt = null,
            ProcessedAt = null,
            Error = null,
        };

        await InTransactionAsync(async (db, session, ct) =>
        {
            await db.GetCollection<OutboxMessage>(_options.OutboxCollection)
                .InsertOneAsync(session, requeued, cancellationToken: ct);
            await db.GetCollection<DeadLetterMessage>(_options.DeadLetterCollection)
                .DeleteOneAsync(session, x => x.Id == deadLetterId, cancellationToken: ct);
        }, cancellationToken);
    }

    private async Task InTransactionAsync(
        Func<IMongoDatabase, IClientSessionHandle, CancellationToken, Task> action,
        CancellationToken cancellationToken)
    {
        var database = await _databaseProvider.GetDatabaseAsync(_options.ConnectionName, cancellationToken);
        using var session = await database.Client.StartSessionAsync(cancellationToken: cancellationToken);

        session.StartTransaction();
        try
        {
            await action(database, session, cancellationToken);
            await session.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await session.AbortTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    private static string Truncate(string value, int max) => value.Length <= max ? value : value[..max];
}
