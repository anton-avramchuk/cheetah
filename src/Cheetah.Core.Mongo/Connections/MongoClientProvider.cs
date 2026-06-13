using System.Collections.Concurrent;
using Cheetah.Core.DependencyInjection;
using MongoDB.Driver;

namespace Cheetah.Core.Mongo.Connections;

/// <summary>
/// Thread-safe <see cref="IMongoClientProvider"/> that keeps one <see cref="IMongoClient"/> per
/// distinct connection string for the lifetime of the application.
/// </summary>
[Export(LifetimeType.Singleton, typeof(IMongoClientProvider))]
public class MongoClientProvider : IMongoClientProvider
{
    private readonly ConcurrentDictionary<string, IMongoClient> _clients = new(StringComparer.Ordinal);

    /// <inheritdoc />
    public IMongoClient GetClient(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        return _clients.GetOrAdd(connectionString, static cs => new MongoClient(cs));
    }
}
