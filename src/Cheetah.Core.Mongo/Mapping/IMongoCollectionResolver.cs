using MongoDB.Driver;

namespace Cheetah.Core.Mongo.Mapping;

/// <summary>
/// Resolves <see cref="IMongoCollection{TDocument}"/> handles. For domain entities the collection
/// name comes from <see cref="IMongoEntityMapRegistry"/> and the database from the entity's
/// <see cref="Cheetah.Core.DataAccess.Attributes.ConnectionStringNameAttribute"/>. An explicit
/// overload serves store POCOs (outbox/inbox/audit/saga) that pick their own collection/database.
/// </summary>
public interface IMongoCollectionResolver
{
    /// <summary>Resolves the collection for a mapped domain entity.</summary>
    ValueTask<IMongoCollection<TEntity>> GetEntityCollectionAsync<TEntity>(CancellationToken cancellationToken = default)
        where TEntity : class;

    /// <summary>Resolves a collection by explicit name and optional connection-string name.</summary>
    ValueTask<IMongoCollection<TDocument>> GetCollectionAsync<TDocument>(
        string collectionName,
        string? connectionName = null,
        CancellationToken cancellationToken = default);
}
