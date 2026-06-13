using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Mongo.Mapping;
using Cheetah.Core.Mongo.UnitOfWork;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Cheetah.Core.Inbox.Mongo;

/// <summary>
/// MongoDB implementation of <see cref="IInboxStore"/>. <see cref="AddAsync"/> buffers the insert on
/// the scoped <see cref="IMongoUnitOfWork"/> so the idempotency record commits in the same
/// transaction as the consumer's work. A unique index on <c>(EventId, ConsumerName)</c> is the
/// backstop against duplicates.
/// </summary>
[Export(LifetimeType.Scoped, typeof(IInboxStore))]
public class MongoInboxStore : IInboxStore
{
    private readonly IMongoCollectionResolver _collectionResolver;
    private readonly IMongoUnitOfWork _unitOfWork;
    private readonly MongoInboxStoreOptions _options;

    /// <summary>Initializes a new instance of the <see cref="MongoInboxStore"/> class.</summary>
    public MongoInboxStore(
        IMongoCollectionResolver collectionResolver,
        IMongoUnitOfWork unitOfWork,
        IOptions<MongoInboxStoreOptions> options)
    {
        _collectionResolver = collectionResolver;
        _unitOfWork = unitOfWork;
        _options = options.Value;
    }

    private ValueTask<IMongoCollection<InboxMessage>> CollectionAsync(CancellationToken ct)
        => _collectionResolver.GetCollectionAsync<InboxMessage>(_options.InboxCollection, _options.ConnectionName, ct);

    /// <inheritdoc />
    public async ValueTask<bool> AlreadyProcessedAsync(Guid eventId, string consumerName, CancellationToken cancellationToken = default)
    {
        var collection = await CollectionAsync(cancellationToken);
        var filter = Builders<InboxMessage>.Filter.And(
            Builders<InboxMessage>.Filter.Eq(x => x.EventId, eventId),
            Builders<InboxMessage>.Filter.Eq(x => x.ConsumerName, consumerName));
        return await collection.Find(filter).Limit(1).AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public ValueTask AddAsync(InboxMessage message, CancellationToken cancellationToken = default)
    {
        _unitOfWork.Enqueue(new PendingWrite
        {
            ConnectionName = _options.ConnectionName,
            ApplyAsync = async (db, session, ct) =>
            {
                await db.GetCollection<InboxMessage>(_options.InboxCollection)
                    .InsertOneAsync(session, message, cancellationToken: ct);
                return 1;
            },
        });
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public async ValueTask<int> DeleteOlderThanAsync(DateTimeOffset olderThan, int batchSize, CancellationToken cancellationToken = default)
    {
        var collection = await CollectionAsync(cancellationToken);
        var ids = await collection.Find(x => x.ReceivedAt < olderThan)
            .SortBy(x => x.ReceivedAt)
            .Limit(batchSize)
            .Project(x => new { x.EventId, x.ConsumerName })
            .ToListAsync(cancellationToken);

        if (ids.Count == 0)
            return 0;

        var filter = Builders<InboxMessage>.Filter.Or(
            ids.Select(k => Builders<InboxMessage>.Filter.And(
                Builders<InboxMessage>.Filter.Eq(x => x.EventId, k.EventId),
                Builders<InboxMessage>.Filter.Eq(x => x.ConsumerName, k.ConsumerName))));

        var result = await collection.DeleteManyAsync(filter, cancellationToken);
        return (int)result.DeletedCount;
    }
}
