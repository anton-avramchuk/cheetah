namespace Cheetah.Core.Dapper.Mapping;

/// <summary>
/// Resolves and caches <see cref="EntityMapping"/> for entity types.
/// Uses an explicitly registered <see cref="IDapperEntityMap"/> when present,
/// otherwise falls back to <see cref="MappingConventions"/>.
/// </summary>
public interface IEntityMapRegistry
{
    /// <summary>Gets the resolved mapping for <typeparamref name="TEntity"/>.</summary>
    EntityMapping GetMapping<TEntity>() where TEntity : class;

    /// <summary>Gets the resolved mapping for the given entity type.</summary>
    EntityMapping GetMapping(Type entityType);
}
