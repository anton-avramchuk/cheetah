using System.Data.Common;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;

namespace Cheetah.Core.Dapper.Connections;

/// <summary>
/// Default <see cref="IDbConnectionFactory"/> that resolves the connection
/// string via <see cref="IConnectionStringResolver"/> and creates the physical
/// connection through the registered <see cref="IDapperConnectionProvider"/>.
/// </summary>
[Export(LifetimeType.Scoped, typeof(IDbConnectionFactory))]
public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly IConnectionStringResolver _connectionStringResolver;
    private readonly IDapperConnectionProvider _connectionProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="DbConnectionFactory"/> class.
    /// </summary>
    public DbConnectionFactory(
        IConnectionStringResolver connectionStringResolver,
        IDapperConnectionProvider connectionProvider)
    {
        _connectionStringResolver = connectionStringResolver;
        _connectionProvider = connectionProvider;
    }

    /// <inheritdoc />
    public async ValueTask<DbConnection> CreateOpenConnectionAsync(
        string? connectionStringName = null,
        CancellationToken cancellationToken = default)
    {
        var connectionString = await _connectionStringResolver.ResolveAsync(connectionStringName);
        var connection = _connectionProvider.Create(connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
