using MongoDB.Driver;

namespace Cheetah.Core.Mongo.Connections;

/// <summary>
/// Caches <see cref="IMongoClient"/> instances per connection string. The MongoDB driver's
/// client owns a connection pool and is designed to be created once and shared, so this is a
/// singleton. Implemented in this core package — MongoDB has a single official driver, so no
/// provider-specific package is required (unlike the Dapper SQL stack).
/// </summary>
public interface IMongoClientProvider
{
    /// <summary>
    /// Returns the shared <see cref="IMongoClient"/> for the given connection string,
    /// creating and caching it on first use.
    /// </summary>
    /// <param name="connectionString">A fully resolved MongoDB connection string (URI).</param>
    IMongoClient GetClient(string connectionString);
}
