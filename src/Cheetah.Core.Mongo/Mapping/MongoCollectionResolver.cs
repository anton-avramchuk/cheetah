using System.Collections.Concurrent;
using System.Reflection;
using Cheetah.Core.DataAccess.Attributes;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Mongo.Connections;
using MongoDB.Driver;

namespace Cheetah.Core.Mongo.Mapping;

/// <summary>
/// Default <see cref="IMongoCollectionResolver"/>.
/// </summary>
[Export(LifetimeType.Scoped, typeof(IMongoCollectionResolver))]
public class MongoCollectionResolver : IMongoCollectionResolver
{
    private static readonly ConcurrentDictionary<Type, string?> ConnectionNames = new();

    private readonly IMongoDatabaseProvider _databaseProvider;
    private readonly IMongoEntityMapRegistry _mapRegistry;

    /// <summary>Initializes a new instance of the <see cref="MongoCollectionResolver"/> class.</summary>
    public MongoCollectionResolver(
        IMongoDatabaseProvider databaseProvider,
        IMongoEntityMapRegistry mapRegistry)
    {
        _databaseProvider = databaseProvider;
        _mapRegistry = mapRegistry;
    }

    /// <inheritdoc />
    public async ValueTask<IMongoCollection<TEntity>> GetEntityCollectionAsync<TEntity>(CancellationToken cancellationToken = default)
        where TEntity : class
    {
        var mapping = _mapRegistry.GetMapping<TEntity>();
        var connectionName = ResolveConnectionName(typeof(TEntity));
        var database = await _databaseProvider.GetDatabaseAsync(connectionName, cancellationToken);
        return database.GetCollection<TEntity>(mapping.CollectionName);
    }

    /// <inheritdoc />
    public async ValueTask<IMongoCollection<TDocument>> GetCollectionAsync<TDocument>(
        string collectionName,
        string? connectionName = null,
        CancellationToken cancellationToken = default)
    {
        var database = await _databaseProvider.GetDatabaseAsync(connectionName, cancellationToken);
        return database.GetCollection<TDocument>(collectionName);
    }

    private static string? ResolveConnectionName(Type entityType)
        => ConnectionNames.GetOrAdd(entityType,
            static t => t.GetCustomAttribute<ConnectionStringNameAttribute>()?.Name);
}
