using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Mongo.Mapping;
using MongoDB.Driver;

namespace Cheetah.Core.Mongo.Querying;

/// <summary>
/// Default <see cref="IMongoQueryExecutor"/> built on <see cref="IMongoCollectionResolver"/>.
/// </summary>
[Export(LifetimeType.Scoped, typeof(IMongoQueryExecutor))]
public class MongoQueryExecutor : IMongoQueryExecutor
{
    private readonly IMongoCollectionResolver _collectionResolver;

    /// <summary>Initializes a new instance of the <see cref="MongoQueryExecutor"/> class.</summary>
    public MongoQueryExecutor(IMongoCollectionResolver collectionResolver)
        => _collectionResolver = collectionResolver;

    /// <inheritdoc />
    public async ValueTask<IReadOnlyList<TResult>> FindAsync<TDocument, TResult>(
        string collectionName,
        FilterDefinition<TDocument> filter,
        ProjectionDefinition<TDocument, TResult> projection,
        string? connectionName = null,
        CancellationToken cancellationToken = default)
    {
        var collection = await _collectionResolver.GetCollectionAsync<TDocument>(collectionName, connectionName, cancellationToken);
        return await collection.Find(filter).Project(projection).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyList<TResult>> AggregateAsync<TDocument, TResult>(
        string collectionName,
        PipelineDefinition<TDocument, TResult> pipeline,
        string? connectionName = null,
        CancellationToken cancellationToken = default)
    {
        var collection = await _collectionResolver.GetCollectionAsync<TDocument>(collectionName, connectionName, cancellationToken);
        var cursor = await collection.AggregateAsync(pipeline, cancellationToken: cancellationToken);
        return await cursor.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async ValueTask<long> CountAsync<TDocument>(
        string collectionName,
        FilterDefinition<TDocument> filter,
        string? connectionName = null,
        CancellationToken cancellationToken = default)
    {
        var collection = await _collectionResolver.GetCollectionAsync<TDocument>(collectionName, connectionName, cancellationToken);
        return await collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
    }
}
