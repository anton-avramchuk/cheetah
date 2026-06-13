using System.Collections.Concurrent;
using Cheetah.Core.DependencyInjection;

namespace Cheetah.Core.Dapper.Mapping;

/// <summary>
/// Default <see cref="IEntityMapRegistry"/>. Indexes all explicitly registered
/// <see cref="IDapperEntityMap"/> instances and lazily builds convention maps for the rest.
/// </summary>
[Export(LifetimeType.Singleton, typeof(IEntityMapRegistry))]
public class EntityMapRegistry : IEntityMapRegistry
{
    private readonly IReadOnlyDictionary<Type, IDapperEntityMap> _explicitMaps;
    private readonly ConcurrentDictionary<Type, EntityMapping> _cache = new();

    /// <summary>
    /// Initializes the registry from the explicitly registered entity maps
    /// (annotate maps with <c>[Export(LifetimeType.Singleton, typeof(IDapperEntityMap))]</c>).
    /// </summary>
    public EntityMapRegistry(IEnumerable<IDapperEntityMap> maps)
    {
        _explicitMaps = maps
            .GroupBy(m => m.EntityType)
            .ToDictionary(g => g.Key, g => g.Last());
    }

    /// <inheritdoc />
    public EntityMapping GetMapping<TEntity>() where TEntity : class
        => GetMapping(typeof(TEntity));

    /// <inheritdoc />
    public EntityMapping GetMapping(Type entityType)
        => _cache.GetOrAdd(entityType, Resolve);

    private EntityMapping Resolve(Type entityType)
    {
        if (_explicitMaps.TryGetValue(entityType, out var map))
            return map.Build();

        var conventionMapType = typeof(ConventionEntityMap<>).MakeGenericType(entityType);
        var conventionMap = (IDapperEntityMap)Activator.CreateInstance(conventionMapType)!;
        return conventionMap.Build();
    }
}
