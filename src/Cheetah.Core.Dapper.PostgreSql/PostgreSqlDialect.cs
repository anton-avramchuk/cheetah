using Cheetah.Core.Dapper.Dialect;
using Cheetah.Core.DependencyInjection;

namespace Cheetah.Core.Dapper.PostgreSql;

/// <summary>
/// PostgreSQL SQL dialect: double-quoted identifiers, <c>@</c> parameters,
/// <c>LIMIT/OFFSET</c> pagination and <c>RETURNING</c> for generated keys —
/// all provided by <see cref="SqlDialectBase"/>, so no overrides are required.
/// </summary>
[Export(LifetimeType.Singleton, typeof(ISqlDialect))]
public sealed class PostgreSqlDialect : SqlDialectBase
{
}
