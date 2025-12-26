namespace Cheetah.Core.Cache;

/// <summary>
/// Unified caching service supporting both in-memory and distributed caching
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a value from cache
    /// </summary>
    ValueTask<T?> GetAsync<T>(string key, CancellationToken ct = default);
    
    /// <summary>
    /// Sets a value in cache
    /// </summary>
    ValueTask SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default);
    
    /// <summary>
    /// Gets a value or sets it if it doesn't exist (Cache-Aside pattern)
    /// </summary>
    ValueTask<T> GetOrSetAsync<T>(string key, Func<ValueTask<T>> factory, TimeSpan? expiry = null, CancellationToken ct = default);
    
    /// <summary>
    /// Removes a value from cache
    /// </summary>
    ValueTask RemoveAsync(string key, CancellationToken ct = default);
}