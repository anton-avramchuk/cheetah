using System.Net;
using Cheetah.AspNetCore.Blazor.Auth.Http;
using Cheetah.AspNetCore.Blazor.Auth.Tokens;
using Moq;

namespace Cheetah.AspNetCore.Blazor.Tests.Auth;

public class BearerTokenHandlerTests
{
    private sealed class StubHandler(params HttpStatusCode[] statuses) : HttpMessageHandler
    {
        private readonly Queue<HttpStatusCode> _statuses = new(statuses);
        public List<string?> AuthHeaders { get; } = [];
        public int Calls { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Calls++;
            AuthHeaders.Add(request.Headers.Authorization?.ToString());
            var status = _statuses.Count > 0 ? _statuses.Dequeue() : HttpStatusCode.OK;
            return Task.FromResult(new HttpResponseMessage(status));
        }
    }

    private static HttpClient Client(BearerTokenHandler handler, StubHandler inner)
    {
        handler.InnerHandler = inner;
        return new HttpClient(handler);
    }

    [Fact]
    public async Task AttachesBearerToken_WhenAvailable()
    {
        var provider = new Mock<IAccessTokenProvider>();
        provider.Setup(p => p.GetAccessTokenAsync(It.IsAny<CancellationToken>())).ReturnsAsync("tok");
        var inner = new StubHandler(HttpStatusCode.OK);

        var resp = await Client(new BearerTokenHandler(provider.Object), inner).GetAsync("http://svc/data");

        resp.StatusCode.ShouldBe(HttpStatusCode.OK);
        inner.Calls.ShouldBe(1);
        inner.AuthHeaders[0].ShouldBe("Bearer tok");
    }

    [Fact]
    public async Task NoToken_SendsWithoutAuthorizationHeader()
    {
        var provider = new Mock<IAccessTokenProvider>();
        provider.Setup(p => p.GetAccessTokenAsync(It.IsAny<CancellationToken>())).ReturnsAsync((string?)null);
        var inner = new StubHandler(HttpStatusCode.OK);

        await Client(new BearerTokenHandler(provider.Object), inner).GetAsync("http://svc/data");

        inner.AuthHeaders[0].ShouldBeNull();
    }

    [Fact]
    public async Task On401_RefreshesAndRetriesOnceWithNewToken()
    {
        var provider = new Mock<IAccessTokenProvider>();
        provider.Setup(p => p.GetAccessTokenAsync(It.IsAny<CancellationToken>())).ReturnsAsync("stale");
        provider.Setup(p => p.RefreshAccessTokenAsync(It.IsAny<CancellationToken>())).ReturnsAsync("fresh");
        var inner = new StubHandler(HttpStatusCode.Unauthorized, HttpStatusCode.OK);

        var resp = await Client(new BearerTokenHandler(provider.Object), inner).GetAsync("http://svc/data");

        resp.StatusCode.ShouldBe(HttpStatusCode.OK);
        inner.Calls.ShouldBe(2);
        inner.AuthHeaders[0].ShouldBe("Bearer stale");
        inner.AuthHeaders[1].ShouldBe("Bearer fresh");
        provider.Verify(p => p.RefreshAccessTokenAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task On401_WhenRefreshFails_DoesNotRetry_ReturnsUnauthorized()
    {
        var provider = new Mock<IAccessTokenProvider>();
        provider.Setup(p => p.GetAccessTokenAsync(It.IsAny<CancellationToken>())).ReturnsAsync("stale");
        provider.Setup(p => p.RefreshAccessTokenAsync(It.IsAny<CancellationToken>())).ReturnsAsync((string?)null);
        var inner = new StubHandler(HttpStatusCode.Unauthorized);

        var resp = await Client(new BearerTokenHandler(provider.Object), inner).GetAsync("http://svc/data");

        resp.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        inner.Calls.ShouldBe(1);
        provider.Verify(p => p.RefreshAccessTokenAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
