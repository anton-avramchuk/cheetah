namespace Cheetah.Core.Dapper.Mapping;

/// <summary>
/// Non-generic marker for entity maps so they can be auto-discovered during DI registration.
/// </summary>
public interface IDapperEntityMap
{
    /// <summary>The entity type this map describes.</summary>
    Type EntityType { get; }

    /// <summary>Builds the resolved <see cref="EntityMapping"/>.</summary>
    EntityMapping Build();
}

/// <summary>
/// Strongly-typed entity map. Implement by deriving from <see cref="DapperEntityMap{TEntity}"/>.
/// </summary>
/// <typeparam name="TEntity">The mapped entity type.</typeparam>
public interface IDapperEntityMap<TEntity> : IDapperEntityMap
{
}
