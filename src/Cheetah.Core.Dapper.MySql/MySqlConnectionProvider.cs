using System.Data.Common;
using Cheetah.Core.Dapper.Connections;
using Cheetah.Core.DependencyInjection;
using MySqlConnector;

namespace Cheetah.Core.Dapper.MySql;

/// <summary>
/// Creates <see cref="MySqlConnection"/> instances (MySqlConnector) for the Dapper data-access layer.
/// </summary>
[Export(LifetimeType.Singleton, typeof(IDapperConnectionProvider))]
public sealed class MySqlConnectionProvider : IDapperConnectionProvider
{
    /// <inheritdoc />
    public DbConnection Create(string connectionString) => new MySqlConnection(connectionString);
}
