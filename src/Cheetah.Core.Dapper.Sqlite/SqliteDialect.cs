using Cheetah.Core.Dapper.Dialect;
using Cheetah.Core.DependencyInjection;

namespace Cheetah.Core.Dapper.Sqlite;

/// <summary>
/// SQLite SQL dialect. Identifiers are double-quoted, parameters use <c>@</c>, pagination uses
/// <c>LIMIT/OFFSET</c> and generated keys use <c>RETURNING</c> (SQLite 3.35+, bundled with
/// Microsoft.Data.Sqlite) — all matching <see cref="SqlDialectBase"/>, so no overrides are needed.
/// </summary>
[Export(LifetimeType.Singleton, typeof(ISqlDialect))]
public sealed class SqliteDialect : SqlDialectBase
{
}
