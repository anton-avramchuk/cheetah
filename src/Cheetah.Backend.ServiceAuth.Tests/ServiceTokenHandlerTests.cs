using System.Net;
using Cheetah.Backend.ServiceAuth;
using Shouldly;

namespace Cheetah.Backend.ServiceAuth.Tests;

public class ServiceTokenHandlerTests
{
    private sealed class StubProvider(string token) : IServiceTokenProvider
    {
        public ValueTask<string> GetTokenAsync(CancellationToken ct = default) => ValueTask.FromResult(token);
    }

    private sealed class CapturingInner : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }

    [Fact]
    public async Task SendAsync_AttachesBearerToken()
    {
        var inner = new CapturingInner();
        var handler = new ServiceTokenHandler(new StubProvider("the-token")) { InnerHandler = inner };
        var invoker = new HttpMessageInvoker(handler);

        await invoker.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, "https://deals.test/api/deals"),
            CancellationToken.None);

        inner.LastRequest!.Headers.Authorization!.Scheme.ShouldBe("Bearer");
        inner.LastRequest.Headers.Authorization.Parameter.ShouldBe("the-token");
    }
}
