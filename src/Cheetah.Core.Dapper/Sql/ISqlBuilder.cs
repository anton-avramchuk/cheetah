using Cheetah.Core.Dapper.Mapping;

namespace Cheetah.Core.Dapper.Sql;

/// <summary>
/// Builds and caches <see cref="EntitySql"/> statement templates from an
/// <see cref="EntityMapping"/>, applying the active <see cref="Dialect.ISqlDialect"/>.
/// </summary>
public interface ISqlBuilder
{
    /// <summary>Returns the cached SQL templates for the given mapping.</summary>
    EntitySql GetSql(EntityMapping mapping);
}
