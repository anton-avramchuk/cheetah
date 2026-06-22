using Cheetah.AspNetCore.Blazor.Auth;
using Cheetah.AspNetCore.Blazor.Auth.Abstractions;
using Cheetah.AspNetCore.Blazor.Auth.Tokens;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;
using Moq;

namespace Cheetah.AspNetCore.Blazor.Tests.Auth;

public class AccessTokenProviderTests
{
    private static readonly DateTimeOffset T0 = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    private static AccessTokenProvider Create(
        AuthenticationStateProvider authState,
        IUserTokenStore store,
        IBffAuthenticator authenticator,
        TimeProvider clock,
        int skewSeconds = 30)
        => new(authState, store, authenticator,
            Options.Create(new BffAuthOptions { RefreshSkewSeconds = skewSeconds }), clock);

    [Fact]
    public async Task GetAccessToken_NoSession_ReturnsNull_AndDoesNotRefresh()
    {
        var auth = new Mock<IBffAuthenticator>(MockBehavior.Strict);
        var sut = Create(FakeAuthStateProvider.Anonymous(), new InMemoryUserTokenStore(), auth.Object, new TestTimeProvider(T0));

        (await sut.GetAccessTokenAsync()).ShouldBeNull();
        auth.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetAccessToken_NoTokensInStore_ReturnsNull()
    {
        var auth = new Mock<IBffAuthenticator>(MockBehavior.Strict);
        var sut = Create(FakeAuthStateProvider.WithSession("s1"), new InMemoryUserTokenStore(), auth.Object, new TestTimeProvider(T0));

        (await sut.GetAccessTokenAsync()).ShouldBeNull();
    }

    [Fact]
    public async Task GetAccessToken_ValidToken_ReturnsIt_WithoutRefresh()
    {
        var store = new InMemoryUserTokenStore();
        await store.StoreAsync("s1", new BffTokenSet("access", "refresh", T0.AddMinutes(5)));
        var auth = new Mock<IBffAuthenticator>(MockBehavior.Strict);

        var sut = Create(FakeAuthStateProvider.WithSession("s1"), store, auth.Object, new TestTimeProvider(T0));

        (await sut.GetAccessTokenAsync()).ShouldBe("access");
        auth.Verify(a => a.RefreshAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAccessToken_ExpiredWithinSkew_RefreshesAndStoresNew()
    {
        var store = new InMemoryUserTokenStore();
        await store.StoreAsync("s1", new BffTokenSet("old", "refresh", T0.AddSeconds(10))); // в пределах skew=30с
        var auth = new Mock<IBffAuthenticator>();
        auth.Setup(a => a.RefreshAsync("refresh", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BffTokenSet("fresh", "refresh2", T0.AddMinutes(5)));

        var sut = Create(FakeAuthStateProvider.WithSession("s1"), store, auth.Object, new TestTimeProvider(T0));

        (await sut.GetAccessTokenAsync()).ShouldBe("fresh");
        (await store.GetAsync("s1"))!.RefreshToken.ShouldBe("refresh2");
        auth.Verify(a => a.RefreshAsync("refresh", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAccessToken_ExpiredAndRefreshFails_RemovesSession_ReturnsNull()
    {
        var store = new InMemoryUserTokenStore();
        await store.StoreAsync("s1", new BffTokenSet("old", "refresh", T0.AddSeconds(-1)));
        var auth = new Mock<IBffAuthenticator>();
        auth.Setup(a => a.RefreshAsync("refresh", It.IsAny<CancellationToken>())).ReturnsAsync((BffTokenSet?)null);

        var sut = Create(FakeAuthStateProvider.WithSession("s1"), store, auth.Object, new TestTimeProvider(T0));

        (await sut.GetAccessTokenAsync()).ShouldBeNull();
        (await store.GetAsync("s1")).ShouldBeNull();
    }

    [Fact]
    public async Task GetAccessToken_ExpiredWithoutRefreshToken_RemovesSession_ReturnsNull()
    {
        var store = new InMemoryUserTokenStore();
        await store.StoreAsync("s1", new BffTokenSet("old", RefreshToken: null, T0.AddSeconds(-1)));
        var auth = new Mock<IBffAuthenticator>(MockBehavior.Strict);

        var sut = Create(FakeAuthStateProvider.WithSession("s1"), store, auth.Object, new TestTimeProvider(T0));

        (await sut.GetAccessTokenAsync()).ShouldBeNull();
        (await store.GetAsync("s1")).ShouldBeNull();
    }

    [Fact]
    public async Task RefreshAccessToken_ForcesRefresh_EvenIfNotExpired()
    {
        var store = new InMemoryUserTokenStore();
        await store.StoreAsync("s1", new BffTokenSet("still-valid", "refresh", T0.AddMinutes(5)));
        var auth = new Mock<IBffAuthenticator>();
        auth.Setup(a => a.RefreshAsync("refresh", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BffTokenSet("forced", "refresh2", T0.AddMinutes(5)));

        var sut = Create(FakeAuthStateProvider.WithSession("s1"), store, auth.Object, new TestTimeProvider(T0));

        (await sut.RefreshAccessTokenAsync()).ShouldBe("forced");
        auth.Verify(a => a.RefreshAsync("refresh", It.IsAny<CancellationToken>()), Times.Once);
    }
}
