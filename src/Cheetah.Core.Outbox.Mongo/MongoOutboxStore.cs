using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Mongo.Mapping;
using Cheetah.Core.Mongo.UnitOfWork;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Cheetah.Core.Outbox.Mongo;

/// <summary>
/// MongoDB implementation of <see cref="IOutboxStore"/>. <see cref="AddAsync"/> buffers the insert
/// on the scoped <see cref="IMongoUnitOfWork"/> so the message commits in the same transaction as
/// the aggregate (mirroring the EF store's deferred <c>Add</c> + shared <c>SaveChanges</c>). The
/// processor methods read/update directly.
/// </summary>
[Export(LifetimeType.Scoped, typeof(IOutboxStore))]
public class MongoOutboxStore : IOutboxStore
{
    private readonly IMongoCollectionResolver _collectionResolver;
    private readonly IMongoUnitOfWork _unitOfWork;
    private readonly MongoOutboxStoreOptions _options;

    /// <summary>Initializes a new instance of the <see cref="MongoOutboxStore"/> class.</summary>
    public MongoOutboxStore(
        IMongoCollectionResolver collectionResolver,
        IMongoUnitOfWork unitOfWork,
        IOptions<MongoOutboxStoreOptions> options)
    {
        _collectionResolver = collectionResolver;
        _unitOfWork = unitOfWork;
        _options = options.Value;
    }

    private ValueTask<IMongoCollection<OutboxMessage>> CollectionAsync(CancellationToken ct)
        => _collectionResolver.GetCollectionAsync<OutboxMessage>(_options.OutboxCollection, _options.ConnectionName, ct);

    /// <inheritdoc />
    public ValueTask AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        _unitOfWork.Enqueue(new PendingWrite
        {
            ConnectionName = _options.ConnectionName,
            ApplyAsync = async (db, session, ct) =>
            {
                await db.GetCollection<OutboxMessage>(_options.OutboxCollection)
                    .InsertOneAsync(session, message, cancellationToken: ct);
                return 1;
            },
        });
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyList<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var collection = await CollectionAsync(cancellationToken);
        var filter = Builders<OutboxMessage>.Filter.And(
            Builders<OutboxMessage>.Filter.Eq(x => x.ProcessedAt, null),
            Builders<OutboxMessage>.Filter.Or(
                Builders<OutboxMessage>.Filter.Eq(x => x.NextAttemptAt, null),
                Builders<OutboxMessage>.Filter.Lte(x => x.NextAttemptAt, now)));

        return await collection.Find(filter)
            .SortBy(x => x.OccurredAt)
            .Limit(batchSize)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async ValueTask MarkProcessedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var collection = await CollectionAsync(cancellationToken);
        var update = Builders<OutboxMessage>.Update
            .Set(x => x.ProcessedAt, DateTimeOffset.UtcNow)
            .Set(x => x.Error, null);
        await collection.UpdateOneAsync(x => x.Id == id, update, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async ValueTask MarkProcessedBatchAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0) return;

        var collection = await CollectionAsync(cancellationToken);
        var update = Builders<OutboxMessage>.Update
            .Set(x => x.ProcessedAt, DateTimeOffset.UtcNow)
            .Set(x => x.Error, null);
        await collection.UpdateManyAsync(
            Builders<OutboxMessage>.Filter.In(x => x.Id, ids), update, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async ValueTask MarkFailedAsync(Guid id, string error, DateTimeOffset nextAttemptAt, CancellationToken cancellationToken = default)
    {
        var collection = await CollectionAsync(cancellationToken);
        var update = Builders<OutboxMessage>.Update
            .Inc(x => x.RetryCount, 1)
            .Set(x => x.Error, Truncate(error, 4000))
            .Set(x => x.NextAttemptAt, nextAttemptAt);
        await collection.UpdateOneAsync(x => x.Id == id, update, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async ValueTask<int> DeleteProcessedAsync(DateTimeOffset olderThan, int batchSize, CancellationToken cancellationToken = default)
    {
        var collection = await CollectionAsync(cancellationToken);
        var filter = Builders<OutboxMessage>.Filter.And(
            Builders<OutboxMessage>.Filter.Ne(x => x.ProcessedAt, null),
            Builders<OutboxMessage>.Filter.Lt(x => x.ProcessedAt, olderThan));

        var ids = await collection.Find(filter)
            .SortBy(x => x.ProcessedAt)
            .Limit(batchSize)
            .Project(x => x.Id)
            .ToListAsync(cancellationToken);

        if (ids.Count == 0)
            return 0;

        var result = await collection.DeleteManyAsync(
            Builders<OutboxMessage>.Filter.In(x => x.Id, ids), cancellationToken);
        return (int)result.DeletedCount;
    }

    private static string Truncate(string value, int max) => value.Length <= max ? value : value[..max];
}
