namespace Cheetah.Core.Dapper.Mapping;

/// <summary>
/// Fallback map used by <see cref="IEntityMapRegistry"/> when no explicit
/// <see cref="DapperEntityMap{TEntity}"/> is registered for an entity.
/// Applies <see cref="MappingConventions"/> only.
/// </summary>
internal sealed class ConventionEntityMap<TEntity> : DapperEntityMap<TEntity>
    where TEntity : class
{
}
