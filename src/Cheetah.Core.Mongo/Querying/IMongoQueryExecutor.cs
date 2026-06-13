using MongoDB.Driver;

namespace Cheetah.Core.Mongo.Querying;

/// <summary>
/// Low-level executor for hand-written MongoDB reads — the fast path for projections, reports and
/// grids. Each call reads directly from the collection and does not enlist in the
/// <see cref="UnitOfWork.IMongoUnitOfWork"/> transaction, which suits stateless CQRS query handlers
/// under high RPS.
/// </summary>
public interface IMongoQueryExecutor
{
    /// <summary>Runs a filter with a projection and returns all matching results.</summary>
    ValueTask<IReadOnlyList<TResult>> FindAsync<TDocument, TResult>(
        string collectionName,
        FilterDefinition<TDocument> filter,
        ProjectionDefinition<TDocument, TResult> projection,
        string? connectionName = null,
        CancellationToken cancellationToken = default);

    /// <summary>Runs an aggregation pipeline and returns all results.</summary>
    ValueTask<IReadOnlyList<TResult>> AggregateAsync<TDocument, TResult>(
        string collectionName,
        PipelineDefinition<TDocument, TResult> pipeline,
        string? connectionName = null,
        CancellationToken cancellationToken = default);

    /// <summary>Counts documents matching the filter.</summary>
    ValueTask<long> CountAsync<TDocument>(
        string collectionName,
        FilterDefinition<TDocument> filter,
        string? connectionName = null,
        CancellationToken cancellationToken = default);
}
