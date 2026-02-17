using System.Collections.Concurrent;
using Cheetah.Core.Cache;
using Cheetah.Core.DependencyInjection;

namespace Cheetah.Blazor.Components.Cache;

[Export(LifetimeType.Singleton, typeof(ICacheService))]
public class InMemoryCacheService : ICacheService
{
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public ValueTask<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        if (_cache.TryGetValue(key, out var entry) && !entry.IsExpired)
            return new ValueTask<T?>((T?)entry.Value);

        if (entry is not null)
            _cache.TryRemove(key, out _);

        return new ValueTask<T?>(default(T));
    }

    public ValueTask SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        var entry = new CacheEntry(value, expiry.HasValue ? DateTime.UtcNow + expiry.Value : null);
        _cache[key] = entry;
        return ValueTask.CompletedTask;
    }

    public async ValueTask<T> GetOrSetAsync<T>(string key, Func<ValueTask<T>> factory, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        if (_cache.TryGetValue(key, out var entry) && !entry.IsExpired)
            return (T)entry.Value!;

        var semaphore = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(ct);
        try
        {
            // Double-check after acquiring lock
            if (_cache.TryGetValue(key, out entry) && !entry.IsExpired)
                return (T)entry.Value!;

            var value = await factory();
            var newEntry = new CacheEntry(value, expiry.HasValue ? DateTime.UtcNow + expiry.Value : null);
            _cache[key] = newEntry;
            return value;
        }
        finally
        {
            semaphore.Release();
        }
    }

    public ValueTask RemoveAsync(string key, CancellationToken ct = default)
    {
        _cache.TryRemove(key, out _);
        _locks.TryRemove(key, out _);
        return ValueTask.CompletedTask;
    }

    private sealed record CacheEntry(object? Value, DateTime? ExpiresAt)
    {
        public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow >= ExpiresAt.Value;
    }
}
