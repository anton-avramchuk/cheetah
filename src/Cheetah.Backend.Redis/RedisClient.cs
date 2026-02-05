using System.Text.Json;
using Cheetah.Core.DependencyInjection;
using StackExchange.Redis;

namespace Cheetah.Backend.Redis;

[Export(LifetimeType.Scoped, typeof(IRedisClient))]
public sealed class RedisClient : IRedisClient
{
    private readonly IRedisConnectionProvider _connectionProvider;

    public RedisClient(IRedisConnectionProvider connectionProvider)
    {
        _connectionProvider = connectionProvider;
    }

    public async Task<T?> GetAsync<T>(string key, string instanceName = "default", CancellationToken ct = default)
    {
        var db = _connectionProvider.GetDatabase(instanceName);
        var value = await db.StringGetAsync(key);

        if (!value.HasValue)
            return default;

        return JsonSerializer.Deserialize<T>((string)value!);
    }

    public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null, string instanceName = "default", CancellationToken ct = default)
    {
        var db = _connectionProvider.GetDatabase(instanceName);
        var serialized = JsonSerializer.Serialize(value);
        if (expiry.HasValue)
            return await db.StringSetAsync(key, serialized, expiry.Value);
        return await db.StringSetAsync(key, serialized);
    }

    public async Task<bool> DeleteAsync(string key, string instanceName = "default", CancellationToken ct = default)
    {
        var db = _connectionProvider.GetDatabase(instanceName);
        return await db.KeyDeleteAsync(key);
    }

    public async Task<bool> ExistsAsync(string key, string instanceName = "default", CancellationToken ct = default)
    {
        var db = _connectionProvider.GetDatabase(instanceName);
        return await db.KeyExistsAsync(key);
    }

    public async Task<IEnumerable<string>> SearchKeysAsync(string pattern, string instanceName = "default", CancellationToken ct = default)
    {
        var connection = _connectionProvider.GetConnection(instanceName);
        var server = connection.GetServer(connection.GetEndPoints().First());

        var keys = new List<string>();
        await foreach (var key in server.KeysAsync(pattern: pattern))
        {
            keys.Add(key.ToString());
        }

        return keys;
    }

    public async Task<Dictionary<string, T?>> GetManyAsync<T>(IEnumerable<string> keys, string instanceName = "default", CancellationToken ct = default)
    {
        var db = _connectionProvider.GetDatabase(instanceName);
        var redisKeys = keys.Select(k => (RedisKey)k).ToArray();
        var values = await db.StringGetAsync(redisKeys);

        var result = new Dictionary<string, T?>();
        for (int i = 0; i < redisKeys.Length; i++)
        {
            if (values[i].HasValue)
            {
                result[redisKeys[i]!] = JsonSerializer.Deserialize<T>((string)values[i]!);
            }
            else
            {
                result[redisKeys[i]!] = default;
            }
        }

        return result;
    }

    public async Task<bool> SetManyAsync<T>(Dictionary<string, T> values, TimeSpan? expiry = null, string instanceName = "default", CancellationToken ct = default)
    {
        var db = _connectionProvider.GetDatabase(instanceName);
        var batch = db.CreateBatch();

        var tasks = new List<Task<bool>>();
        foreach (var kvp in values)
        {
            var serialized = JsonSerializer.Serialize(kvp.Value);
            if (expiry.HasValue)
                tasks.Add(batch.StringSetAsync(kvp.Key, serialized, expiry.Value));
            else
                tasks.Add(batch.StringSetAsync(kvp.Key, serialized));
        }

        batch.Execute();
        var results = await Task.WhenAll(tasks);
        return results.All(r => r);
    }
}
