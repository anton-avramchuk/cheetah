using Microsoft.Extensions.Caching.Memory;

namespace Cheetah.Core.Cache;

/// <summary>
/// Реализация <see cref="ICacheService"/> поверх <see cref="IMemoryCache"/> — процесс-локальный кэш без
/// внешней зависимости (Redis и т.п.). Регистрируется <see cref="CrmCacheCoreModule"/> через
/// <c>TryAdd</c> как безопасный дефолт: хост, которому нужен распределённый кэш (например, общий между
/// инстансами), может перекрыть регистрацию своей реализацией ДО инициализации модулей.
/// </summary>
public sealed class MemoryCacheService : ICacheService
{
    private static readonly TimeSpan DefaultExpiry = TimeSpan.FromMinutes(5);

    private readonly IMemoryCache _cache;

    public MemoryCacheService(IMemoryCache cache) => _cache = cache;

    public ValueTask<T?> GetAsync<T>(string key, CancellationToken ct = default)
        => ValueTask.FromResult(_cache.TryGetValue(key, out T? value) ? value : default);

    public ValueTask SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        _cache.Set(key, value, expiry ?? DefaultExpiry);
        return ValueTask.CompletedTask;
    }

    public async ValueTask<T> GetOrSetAsync<T>(
        string key, Func<ValueTask<T>> factory, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        if (_cache.TryGetValue(key, out T? cached) && cached is not null)
            return cached;

        var value = await factory();
        _cache.Set(key, value, expiry ?? DefaultExpiry);
        return value;
    }

    public ValueTask RemoveAsync(string key, CancellationToken ct = default)
    {
        _cache.Remove(key);
        return ValueTask.CompletedTask;
    }
}
