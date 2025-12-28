using Cheetah.Core.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Cheetah.Backend.Redis;

[Export(LifetimeType.Singleton, typeof(IRedisConnectionProvider))]
public sealed class RedisConnectionProvider : IRedisConnectionProvider, IDisposable
{
    private readonly RedisOptions _options;
    private readonly Dictionary<string, Lazy<ConnectionMultiplexer>> _connections = new();
    private readonly object _lock = new();

    public RedisConnectionProvider(IOptions<RedisOptions> options)
    {
        _options = options.Value;
        InitializeConnections();
    }

    private void InitializeConnections()
    {
        foreach (var instance in _options.Instances)
        {
            _connections[instance.Key] = new Lazy<ConnectionMultiplexer>(() =>
                ConnectionMultiplexer.Connect(instance.Value.ConnectionString));
        }
    }

    public IConnectionMultiplexer GetConnection(string instanceName = "default")
    {
        if (!_connections.TryGetValue(instanceName, out var connection))
        {
            throw new InvalidOperationException($"Redis instance '{instanceName}' is not configured.");
        }

        return connection.Value;
    }

    public IDatabase GetDatabase(string instanceName = "default")
    {
        var connection = GetConnection(instanceName);
        var instanceOptions = _options.Instances[instanceName];
        return connection.GetDatabase(instanceOptions.Database);
    }

    public void Dispose()
    {
        foreach (var connection in _connections.Values.Where(c => c.IsValueCreated))
        {
            connection.Value.Dispose();
        }
    }
}
