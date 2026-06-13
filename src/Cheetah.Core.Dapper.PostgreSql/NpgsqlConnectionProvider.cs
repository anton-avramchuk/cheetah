using System.Data.Common;
using Cheetah.Core.Dapper.Connections;
using Cheetah.Core.DependencyInjection;
using Npgsql;

namespace Cheetah.Core.Dapper.PostgreSql;

/// <summary>
/// Creates <see cref="NpgsqlConnection"/> instances for the Dapper data-access layer.
/// </summary>
[Export(LifetimeType.Singleton, typeof(IDapperConnectionProvider))]
public sealed class NpgsqlConnectionProvider : IDapperConnectionProvider
{
    /// <inheritdoc />
    public DbConnection Create(string connectionString) => new NpgsqlConnection(connectionString);
}
