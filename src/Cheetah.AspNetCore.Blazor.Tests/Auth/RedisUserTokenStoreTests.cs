using Cheetah.AspNetCore.Blazor.Auth;
using Cheetah.AspNetCore.Blazor.Auth.Abstractions;
using Cheetah.AspNetCore.Blazor.Auth.Redis;
using Cheetah.Backend.Redis;
using Microsoft.Extensions.Options;
using Moq;

namespace Cheetah.AspNetCore.Blazor.Tests.Auth;

public class RedisUserTokenStoreTests
{
    private static readonly BffTokenSet Sample =
        new("access", "refresh", new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero));

    private static RedisUserTokenStore Create(
        IRedisClient redis,
        RedisUserTokenStoreOptions? redisOptions = null,
        BffAuthOptions? bffOptions = null)
        => new(redis,
            Options.Create(redisOptions ?? new RedisUserTokenStoreOptions()),
            Options.Create(bffOptions ?? new BffAuthOptions()));

    [Fact]
    public async Task Store_WritesPrefixedKey_WithTokens()
    {
        var redis = new Mock<IRedisClient>();
        string? key = null;
        BffTokenSet? value = null;
        redis.Setup(r => r.SetAsync(It.IsAny<string>(), It.IsAny<BffTokenSet>(), It.IsAny<TimeSpan?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, BffTokenSet, TimeSpan?, string, CancellationToken>((k, v, _, _, _) => { key = k; value = v; })
            .ReturnsAsync(true);

        await Create(redis.Object).StoreAsync("sess1", Sample);

        key.ShouldBe("bff:tokens:sess1");
        value.ShouldBe(Sample);
    }

    [Fact]
    public async Task Store_UsesBffExpireTimeSpan_AsTtl_WhenNotOverridden()
    {
        var redis = new Mock<IRedisClient>();
        TimeSpan? ttl = null;
        redis.Setup(r => r.SetAsync(It.IsAny<string>(), It.IsAny<BffTokenSet>(), It.IsAny<TimeSpan?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, BffTokenSet, TimeSpan?, string, CancellationToken>((_, _, t, _, _) => ttl = t)
            .ReturnsAsync(true);

        await Create(redis.Object, bffOptions: new BffAuthOptions { ExpireTimeSpan = TimeSpan.FromHours(3) })
            .StoreAsync("s", Sample);

        ttl.ShouldBe(TimeSpan.FromHours(3));
    }

    [Fact]
    public async Task Store_UsesExplicitTtl_WhenConfigured()
    {
        var redis = new Mock<IRedisClient>();
        TimeSpan? ttl = null;
        redis.Setup(r => r.SetAsync(It.IsAny<string>(), It.IsAny<BffTokenSet>(), It.IsAny<TimeSpan?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, BffTokenSet, TimeSpan?, string, CancellationToken>((_, _, t, _, _) => ttl = t)
            .ReturnsAsync(true);

        await Create(redis.Object, new RedisUserTokenStoreOptions { Ttl = TimeSpan.FromMinutes(15) })
            .StoreAsync("s", Sample);

        ttl.ShouldBe(TimeSpan.FromMinutes(15));
    }

    [Fact]
    public async Task Store_AppliesCustomPrefixAndInstance()
    {
        var redis = new Mock<IRedisClient>();
        string? key = null;
        string? instance = null;
        redis.Setup(r => r.SetAsync(It.IsAny<string>(), It.IsAny<BffTokenSet>(), It.IsAny<TimeSpan?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, BffTokenSet, TimeSpan?, string, CancellationToken>((k, _, _, i, _) => { key = k; instance = i; })
            .ReturnsAsync(true);

        await Create(redis.Object, new RedisUserTokenStoreOptions { KeyPrefix = "tok:", InstanceName = "sessions" })
            .StoreAsync("abc", Sample);

        key.ShouldBe("tok:abc");
        instance.ShouldBe("sessions");
    }

    [Fact]
    public async Task Get_ReadsPrefixedKey_AndReturnsTokens()
    {
        var redis = new Mock<IRedisClient>();
        redis.Setup(r => r.GetAsync<BffTokenSet>("bff:tokens:sess1", "default", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Sample);

        var result = await Create(redis.Object).GetAsync("sess1");

        result.ShouldBe(Sample);
    }

    [Fact]
    public async Task Get_ReturnsNull_WhenMissing()
    {
        var redis = new Mock<IRedisClient>();
        redis.Setup(r => r.GetAsync<BffTokenSet>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BffTokenSet?)null);

        (await Create(redis.Object).GetAsync("missing")).ShouldBeNull();
    }

    [Fact]
    public async Task Remove_DeletesPrefixedKey()
    {
        var redis = new Mock<IRedisClient>();
        redis.Setup(r => r.DeleteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Create(redis.Object).RemoveAsync("sess1");

        redis.Verify(r => r.DeleteAsync("bff:tokens:sess1", "default", It.IsAny<CancellationToken>()), Times.Once);
    }
}
