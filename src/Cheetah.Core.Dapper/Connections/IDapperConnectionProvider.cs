using System.Data.Common;

namespace Cheetah.Core.Dapper.Connections;

/// <summary>
/// Provider-specific factory that creates a concrete ADO.NET connection
/// (e.g. <c>NpgsqlConnection</c>) for an already-resolved connection string.
/// Implemented in the database-specific package (PostgreSql, Sqlite, ...).
/// </summary>
public interface IDapperConnectionProvider
{
    /// <summary>
    /// Creates a new, unopened <see cref="DbConnection"/> for the given connection string.
    /// </summary>
    /// <param name="connectionString">The fully resolved connection string.</param>
    DbConnection Create(string connectionString);
}
