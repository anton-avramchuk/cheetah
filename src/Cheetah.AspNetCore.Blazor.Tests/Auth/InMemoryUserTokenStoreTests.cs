using Cheetah.AspNetCore.Blazor.Auth.Abstractions;
using Cheetah.AspNetCore.Blazor.Auth.Tokens;

namespace Cheetah.AspNetCore.Blazor.Tests.Auth;

public class InMemoryUserTokenStoreTests
{
    private static BffTokenSet Tokens(string access = "a")
        => new(access, "r", DateTimeOffset.UtcNow.AddMinutes(5));

    [Fact]
    public async Task Store_Then_Get_ReturnsSameTokens()
    {
        var store = new InMemoryUserTokenStore();
        var tokens = Tokens();

        await store.StoreAsync("s1", tokens);

        (await store.GetAsync("s1")).ShouldBe(tokens);
    }

    [Fact]
    public async Task Get_UnknownSession_ReturnsNull()
    {
        var store = new InMemoryUserTokenStore();
        (await store.GetAsync("missing")).ShouldBeNull();
    }

    [Fact]
    public async Task Store_OverwritesPreviousTokens()
    {
        var store = new InMemoryUserTokenStore();
        await store.StoreAsync("s1", Tokens("old"));

        await store.StoreAsync("s1", Tokens("new"));

        (await store.GetAsync("s1"))!.AccessToken.ShouldBe("new");
    }

    [Fact]
    public async Task Remove_DeletesTokens()
    {
        var store = new InMemoryUserTokenStore();
        await store.StoreAsync("s1", Tokens());

        await store.RemoveAsync("s1");

        (await store.GetAsync("s1")).ShouldBeNull();
    }

    [Fact]
    public async Task Sessions_AreIsolated()
    {
        var store = new InMemoryUserTokenStore();
        await store.StoreAsync("s1", Tokens("one"));
        await store.StoreAsync("s2", Tokens("two"));

        (await store.GetAsync("s1"))!.AccessToken.ShouldBe("one");
        (await store.GetAsync("s2"))!.AccessToken.ShouldBe("two");
    }
}
