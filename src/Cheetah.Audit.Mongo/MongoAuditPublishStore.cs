using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Mongo.Mapping;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Cheetah.Audit.Mongo;

/// <summary>
/// MongoDB <see cref="IAuditPublishStore"/> used by the Kafka publisher to read unpublished audit
/// entries and record their outcome.
/// </summary>
[Export(LifetimeType.Scoped, typeof(IAuditPublishStore))]
public class MongoAuditPublishStore : IAuditPublishStore
{
    private readonly IMongoCollectionResolver _collectionResolver;
    private readonly MongoAuditStoreOptions _options;

    /// <summary>Initializes a new instance of the <see cref="MongoAuditPublishStore"/> class.</summary>
    public MongoAuditPublishStore(
        IMongoCollectionResolver collectionResolver,
        IOptions<MongoAuditStoreOptions> options)
    {
        _collectionResolver = collectionResolver;
        _options = options.Value;
    }

    private ValueTask<IMongoCollection<AuditEntry>> CollectionAsync(CancellationToken ct)
        => _collectionResolver.GetCollectionAsync<AuditEntry>(_options.AuditCollection, _options.ConnectionName, ct);

    /// <inheritdoc />
    public async ValueTask<IReadOnlyList<AuditEntry>> ClaimPendingAsync(int batchSize, TimeSpan claimTimeout, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var claimUntil = now + claimTimeout;
        var collection = await CollectionAsync(cancellationToken);

        var filter = Builders<AuditEntry>.Filter.And(
            Builders<AuditEntry>.Filter.Eq(x => x.PublishedAt, null),
            Builders<AuditEntry>.Filter.Or(
                Builders<AuditEntry>.Filter.Eq(x => x.NextAttemptAt, null),
                Builders<AuditEntry>.Filter.Lte(x => x.NextAttemptAt, now)));

        var pending = await collection.Find(filter)
            .SortBy(x => x.OccurredAt)
            .Limit(batchSize)
            .ToListAsync(cancellationToken);

        if (pending.Count == 0)
            return pending;

        // Claim: push NextAttemptAt forward so another replica skips these for the timeout window.
        var ids = pending.Select(p => p.Id).ToList();
        await collection.UpdateManyAsync(
            Builders<AuditEntry>.Filter.In(x => x.Id, ids),
            Builders<AuditEntry>.Update.Set(x => x.NextAttemptAt, claimUntil),
            cancellationToken: cancellationToken);

        return pending;
    }

    /// <inheritdoc />
    public async ValueTask MarkPublishedAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0) return;

        var collection = await CollectionAsync(cancellationToken);
        var update = Builders<AuditEntry>.Update
            .Set(x => x.PublishedAt, DateTimeOffset.UtcNow)
            .Set(x => x.Error, null);
        await collection.UpdateManyAsync(
            Builders<AuditEntry>.Filter.In(x => x.Id, ids), update, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async ValueTask MarkFailedAsync(Guid id, string error, DateTimeOffset nextAttemptAt, CancellationToken cancellationToken = default)
    {
        var collection = await CollectionAsync(cancellationToken);
        var update = Builders<AuditEntry>.Update
            .Inc(x => x.RetryCount, 1)
            .Set(x => x.Error, Truncate(error, 4000))
            .Set(x => x.NextAttemptAt, nextAttemptAt);
        await collection.UpdateOneAsync(x => x.Id == id, update, cancellationToken: cancellationToken);
    }

    private static string Truncate(string value, int max) => value.Length <= max ? value : value[..max];
}
