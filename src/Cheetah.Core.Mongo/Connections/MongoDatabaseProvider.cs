using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using MongoDB.Driver;

namespace Cheetah.Core.Mongo.Connections;

/// <summary>
/// Default <see cref="IMongoDatabaseProvider"/>. Resolves the connection string via
/// <see cref="IConnectionStringResolver"/>, obtains the shared client from
/// <see cref="IMongoClientProvider"/>, and selects the database named in the connection string.
/// </summary>
[Export(LifetimeType.Scoped, typeof(IMongoDatabaseProvider))]
public class MongoDatabaseProvider : IMongoDatabaseProvider
{
    private readonly IConnectionStringResolver _connectionStringResolver;
    private readonly IMongoClientProvider _clientProvider;

    /// <summary>Initializes a new instance of the <see cref="MongoDatabaseProvider"/> class.</summary>
    public MongoDatabaseProvider(
        IConnectionStringResolver connectionStringResolver,
        IMongoClientProvider clientProvider)
    {
        _connectionStringResolver = connectionStringResolver;
        _clientProvider = clientProvider;
    }

    /// <inheritdoc />
    public async ValueTask<IMongoDatabase> GetDatabaseAsync(string? connectionName = null, CancellationToken cancellationToken = default)
    {
        var connectionString = await _connectionStringResolver.ResolveAsync(connectionName)
            ?? throw new InvalidOperationException(
                $"No MongoDB connection string resolved for '{connectionName ?? "Default"}'.");

        var url = MongoUrl.Create(connectionString);
        if (string.IsNullOrWhiteSpace(url.DatabaseName))
            throw new InvalidOperationException(
                $"MongoDB connection string for '{connectionName ?? "Default"}' does not specify a database name. " +
                "Include the database in the URI, e.g. 'mongodb://host:27017/mydb'.");

        var client = _clientProvider.GetClient(connectionString);
        return client.GetDatabase(url.DatabaseName);
    }
}
