using System.Data.Common;

namespace Cheetah.Core.Dapper.Connections;

/// <summary>
/// Creates open ADO.NET connections by named connection string.
/// Resolves the connection string through the shared
/// <see cref="Cheetah.Core.DataAccess.Abstractions.IConnectionStringResolver"/>
/// and delegates connection creation to the provider-specific
/// <see cref="IDapperConnectionProvider"/>.
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>
    /// Resolves the connection string and returns a newly opened connection.
    /// The caller owns the connection and must dispose it.
    /// </summary>
    /// <param name="connectionStringName">
    /// Logical connection string name; <c>null</c> uses the default connection string.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    ValueTask<DbConnection> CreateOpenConnectionAsync(
        string? connectionStringName = null,
        CancellationToken cancellationToken = default);
}
