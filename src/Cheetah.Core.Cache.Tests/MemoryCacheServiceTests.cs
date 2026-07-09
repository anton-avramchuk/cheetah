using Microsoft.Extensions.Caching.Memory;
using Shouldly;

namespace Cheetah.Core.Cache.Tests;

/// <summary>
/// Регресс: <see cref="ICacheService"/> раньше не имел ни одной реализации во всём Cheetah, хотя
/// FeatureManagement.Infrastructure (и CustomFields) требуют её жёстко — любой хост, подключающий их
/// без своего кэша, падал при валидации DI. <see cref="MemoryCacheService"/> — безопасный дефолт.
/// </summary>
public class MemoryCacheServiceTests
{
    private static MemoryCacheService NewService() => new(new MemoryCache(new MemoryCacheOptions()));

    [Fact]
    public async Task GetAsync_returns_default_for_missing_key()
    {
        var sut = NewService();

        (await sut.GetAsync<string>("missing")).ShouldBeNull();
    }

    [Fact]
    public async Task SetAsync_then_GetAsync_returns_value()
    {
        var sut = NewService();

        await sut.SetAsync("key", "value");

        (await sut.GetAsync<string>("key")).ShouldBe("value");
    }

    [Fact]
    public async Task GetOrSetAsync_calls_factory_once_and_caches()
    {
        var sut = NewService();
        var calls = 0;

        async ValueTask<int> Factory()
        {
            calls++;
            return 42;
        }

        var first = await sut.GetOrSetAsync("k", Factory);
        var second = await sut.GetOrSetAsync("k", Factory);

        first.ShouldBe(42);
        second.ShouldBe(42);
        calls.ShouldBe(1);
    }

    [Fact]
    public async Task RemoveAsync_evicts_value()
    {
        var sut = NewService();
        await sut.SetAsync("key", "value");

        await sut.RemoveAsync("key");

        (await sut.GetAsync<string>("key")).ShouldBeNull();
    }
}
