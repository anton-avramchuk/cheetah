namespace Cheetah.Backend.Redis;

public interface IRedisClient
{
    Task<T?> GetAsync<T>(string key, string instanceName = "default", CancellationToken ct = default);

    Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null, string instanceName = "default", CancellationToken ct = default);

    Task<bool> DeleteAsync(string key, string instanceName = "default", CancellationToken ct = default);

    Task<bool> ExistsAsync(string key, string instanceName = "default", CancellationToken ct = default);

    Task<IEnumerable<string>> SearchKeysAsync(string pattern, string instanceName = "default", CancellationToken ct = default);

    Task<Dictionary<string, T?>> GetManyAsync<T>(IEnumerable<string> keys, string instanceName = "default", CancellationToken ct = default);

    Task<bool> SetManyAsync<T>(Dictionary<string, T> values, TimeSpan? expiry = null, string instanceName = "default", CancellationToken ct = default);
}
