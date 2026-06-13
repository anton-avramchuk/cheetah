using System.Data.Common;
using Cheetah.Core.Dapper.Connections;
using Cheetah.Core.DependencyInjection;
using Microsoft.Data.Sqlite;

namespace Cheetah.Core.Dapper.Sqlite;

/// <summary>
/// Creates <see cref="SqliteConnection"/> instances for the Dapper data-access layer.
/// </summary>
[Export(LifetimeType.Singleton, typeof(IDapperConnectionProvider))]
public sealed class SqliteConnectionProvider : IDapperConnectionProvider
{
    /// <inheritdoc />
    public DbConnection Create(string connectionString) => new SqliteConnection(connectionString);
}
