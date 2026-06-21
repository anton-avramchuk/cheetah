using Cheetah.Backend.Redis;
using Cheetah.RateLimit.Redis;
using Microsoft.Extensions.Options;
using Shouldly;
using StackExchange.Redis;
using Testcontainers.Redis;

namespace Cheetah.RateLimit.Redis.Tests;

public class RedisDistributedRateLimiterTests : IAsyncLifetime
{
    private readonly RedisContainer _container = new RedisBuilder("redis:7-alpine")
        .Build();

    private RedisDistributedRateLimiter _sut = null!;
    private IConnectionMultiplexer _mux = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        _mux = await ConnectionMultiplexer.ConnectAsync(_container.GetConnectionString());

        var providerStub = new StubConnectionProvider(_mux);
        _sut = new RedisDistributedRateLimiter(
            providerStub,
            Options.Create(new RedisRateLimitOptions { KeyPrefix = "test:rl:" }));
    }

    public async Task DisposeAsync()
    {
        _mux.Dispose();
        await _container.DisposeAsync();
    }

    [Fact]
    public async Task Within_Limit_All_Attempts_Pass()
    {
        var key = $"user:{Guid.NewGuid()}";
        for (var i = 0; i < 5; i++)
        {
            var d = await _sut.AcquireAsync(key, limit: 5, window: TimeSpan.FromSeconds(60));
            d.IsAllowed.ShouldBeTrue($"Попытка {i + 1} должна быть allowed");
        }
    }

    [Fact]
    public async Task Exceeding_Limit_Returns_NotAllowed_With_RetryAfter()
    {
        var key = $"user:{Guid.NewGuid()}";
        for (var i = 0; i < 3; i++)
            await _sut.AcquireAsync(key, limit: 3, window: TimeSpan.FromSeconds(60));

        var blocked = await _sut.AcquireAsync(key, limit: 3, window: TimeSpan.FromSeconds(60));
        blocked.IsAllowed.ShouldBeFalse();
        blocked.RetryAfter.ShouldBeGreaterThan(TimeSpan.Zero);
        blocked.RemainingPermits.ShouldBe(0);
    }

    [Fact]
    public async Task RemainingPermits_Decreases_With_Each_Attempt()
    {
        var key = $"user:{Guid.NewGuid()}";

        var first = await _sut.AcquireAsync(key, limit: 5, window: TimeSpan.FromSeconds(60));
        first.RemainingPermits.ShouldBe(4);

        var second = await _sut.AcquireAsync(key, limit: 5, window: TimeSpan.FromSeconds(60));
        second.RemainingPermits.ShouldBe(3);
    }

    [Fact]
    public async Task Different_Keys_Do_Not_Affect_Each_Other()
    {
        var keyA = $"user-a:{Guid.NewGuid()}";
        var keyB = $"user-b:{Guid.NewGuid()}";

        for (var i = 0; i < 3; i++)
            await _sut.AcquireAsync(keyA, limit: 3, window: TimeSpan.FromSeconds(60));

        // keyA уже исчерпал, но keyB должен пройти
        var resultB = await _sut.AcquireAsync(keyB, limit: 3, window: TimeSpan.FromSeconds(60));
        resultB.IsAllowed.ShouldBeTrue();
    }

    [Fact]
    public async Task Sliding_Window_Frees_Capacity_Over_Time()
    {
        var key = $"user:{Guid.NewGuid()}";
        // Маленькое окно для теста
        await _sut.AcquireAsync(key, limit: 2, window: TimeSpan.FromSeconds(1));
        await _sut.AcquireAsync(key, limit: 2, window: TimeSpan.FromSeconds(1));

        var blocked = await _sut.AcquireAsync(key, limit: 2, window: TimeSpan.FromSeconds(1));
        blocked.IsAllowed.ShouldBeFalse();

        // Ждём почти полный window — capacity начинает освобождаться (sliding)
        await Task.Delay(1500);

        var afterWait = await _sut.AcquireAsync(key, limit: 2, window: TimeSpan.FromSeconds(1));
        afterWait.IsAllowed.ShouldBeTrue();
    }

    [Fact]
    public async Task NamedPolicy_Uses_Configuration_From_Options()
    {
        var providerStub = new StubConnectionProvider(_mux);
        var sutWithPolicy = new RedisDistributedRateLimiter(
            providerStub,
            Options.Create(new RedisRateLimitOptions
            {
                KeyPrefix = "test:rl:policy:",
                Policies = { ["login"] = new RateLimitPolicy { Limit = 2, Window = TimeSpan.FromSeconds(60) } }
            }));

        var key = $"u:{Guid.NewGuid()}";
        (await sutWithPolicy.AcquireAsync("login", key)).IsAllowed.ShouldBeTrue();
        (await sutWithPolicy.AcquireAsync("login", key)).IsAllowed.ShouldBeTrue();
        (await sutWithPolicy.AcquireAsync("login", key)).IsAllowed.ShouldBeFalse();
    }

    [Fact]
    public async Task NamedPolicy_Throws_When_Policy_Not_Configured()
    {
        await Should.ThrowAsync<InvalidOperationException>(
            () => _sut.AcquireAsync("nonexistent", "key").AsTask());
    }

    private sealed class StubConnectionProvider : IRedisConnectionProvider
    {
        private readonly IConnectionMultiplexer _mux;
        public StubConnectionProvider(IConnectionMultiplexer mux) => _mux = mux;
        public IConnectionMultiplexer GetConnection(string instanceName = "default") => _mux;
        public IDatabase GetDatabase(string instanceName = "default") => _mux.GetDatabase();
    }
}
