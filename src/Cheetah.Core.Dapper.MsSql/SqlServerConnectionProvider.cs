using System.Data.Common;
using Cheetah.Core.Dapper.Connections;
using Cheetah.Core.DependencyInjection;
using Microsoft.Data.SqlClient;

namespace Cheetah.Core.Dapper.MsSql;

/// <summary>
/// Creates <see cref="SqlConnection"/> instances for the Dapper data-access layer.
/// </summary>
[Export(LifetimeType.Singleton, typeof(IDapperConnectionProvider))]
public sealed class SqlServerConnectionProvider : IDapperConnectionProvider
{
    /// <inheritdoc />
    public DbConnection Create(string connectionString) => new SqlConnection(connectionString);
}
