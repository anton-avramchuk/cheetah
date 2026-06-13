using MongoDB.Driver;

namespace Cheetah.Core.Mongo.Connections;

/// <summary>
/// Resolves the <see cref="IMongoDatabase"/> for a logical connection-string name. The connection
/// string is resolved through the shared
/// <see cref="Cheetah.Core.DataAccess.Abstractions.IConnectionStringResolver"/> (same mechanism the
/// EF/Dapper stacks use), and the database name is taken from the connection string's
/// <c>MongoUrl.DatabaseName</c>.
/// </summary>
public interface IMongoDatabaseProvider
{
    /// <summary>
    /// Resolves and returns the database for the given logical connection-string name
    /// (<c>null</c> uses the default connection string).
    /// </summary>
    ValueTask<IMongoDatabase> GetDatabaseAsync(string? connectionName = null, CancellationToken cancellationToken = default);
}
