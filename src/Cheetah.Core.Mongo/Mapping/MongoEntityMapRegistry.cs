using System.Collections.Concurrent;
using Cheetah.Core.DependencyInjection;
using MongoDB.Bson.Serialization;

namespace Cheetah.Core.Mongo.Mapping;

/// <summary>
/// Default <see cref="IMongoEntityMapRegistry"/>. Caches resolved mappings, registers each entity's
/// <see cref="BsonClassMap"/> exactly once (idempotent and thread-safe), and ensures the global
/// Cheetah conventions are registered before any class map.
/// </summary>
[Export(LifetimeType.Singleton, typeof(IMongoEntityMapRegistry))]
public class MongoEntityMapRegistry : IMongoEntityMapRegistry
{
    private readonly Dictionary<Type, IMongoEntityMap> _explicitMaps;
    private readonly ConcurrentDictionary<Type, MongoEntityMapping> _cache = new();
    private readonly object _registrationGate = new();

    /// <summary>Initializes a new instance of the <see cref="MongoEntityMapRegistry"/> class.</summary>
    public MongoEntityMapRegistry(IEnumerable<IMongoEntityMap> maps)
    {
        MongoMappingConventions.EnsureRegistered();
        _explicitMaps = maps.ToDictionary(m => m.EntityType);
    }

    /// <inheritdoc />
    public MongoEntityMapping GetMapping<TEntity>() where TEntity : class => GetMapping(typeof(TEntity));

    /// <inheritdoc />
    public MongoEntityMapping GetMapping(Type entityType)
        => _cache.GetOrAdd(entityType, Resolve);

    private MongoEntityMapping Resolve(Type entityType)
    {
        if (_explicitMaps.TryGetValue(entityType, out var map))
        {
            EnsureClassMapRegistered(entityType, map.BuildClassMap);
            return map.BuildMapping();
        }

        // Convention fallback: lazily auto-map (conventions handle DomainEvents/extra elements).
        EnsureClassMapRegistered(entityType, () => CreateAutoClassMap(entityType));

        return new MongoEntityMapping
        {
            EntityType = entityType,
            CollectionName = MongoMappingConventions.GetCollectionName(entityType),
            KeyMemberName = "Id",
        };
    }

    private void EnsureClassMapRegistered(Type entityType, Func<BsonClassMap> factory)
    {
        if (BsonClassMap.IsClassMapRegistered(entityType))
            return;

        lock (_registrationGate)
        {
            if (BsonClassMap.IsClassMapRegistered(entityType))
                return;

            BsonClassMap.RegisterClassMap(factory());
        }
    }

    private static BsonClassMap CreateAutoClassMap(Type entityType)
    {
        var classMap = new BsonClassMap(entityType);
        classMap.AutoMap();
        return classMap;
    }
}
