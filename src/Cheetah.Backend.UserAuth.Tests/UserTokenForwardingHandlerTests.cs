using System.Net;
using System.Net.Http.Headers;
using Cheetah.Backend.UserAuth;
using Microsoft.AspNetCore.Http;
using Shouldly;

namespace Cheetah.Backend.UserAuth.Tests;

public class UserTokenForwardingHandlerTests
{
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

    private static IHttpContextAccessor Accessor(string? authorization)
    {
        var ctx = new DefaultHttpContext();
        if (authorization is not null)
            ctx.Request.Headers.Authorization = authorization;
        return new HttpContextAccessor { HttpContext = ctx };
    }

    private static async Task<HttpRequestMessage> SendAsync(
        IHttpContextAccessor accessor, HttpRequestMessage request)
    {
        var inner = new CapturingInner();
        var handler = new UserTokenForwardingHandler(accessor) { InnerHandler = inner };
        var invoker = new HttpMessageInvoker(handler);
        await invoker.SendAsync(request, CancellationToken.None);
        return inner.LastRequest!;
    }

    [Fact]
    public async Task ForwardsIncomingAuthorizationHeader()
    {
        var sent = await SendAsync(
            Accessor("Bearer user.jwt.token"),
            new HttpRequestMessage(HttpMethod.Get, "https://deals.test/api/deals"));

        sent.Headers.Authorization!.ToString().ShouldBe("Bearer user.jwt.token");
    }

    [Fact]
    public async Task DoesNotOverwriteExistingAuthorization()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "https://deals.test/api/deals");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "service.token");

        var sent = await SendAsync(Accessor("Bearer user.jwt.token"), request);

        sent.Headers.Authorization!.Parameter.ShouldBe("service.token");
    }

    [Fact]
    public async Task NoHeader_WhenNoIncomingAuthorization()
    {
        var sent = await SendAsync(
            Accessor(authorization: null),
            new HttpRequestMessage(HttpMethod.Get, "https://deals.test/api/deals"));

        sent.Headers.Authorization.ShouldBeNull();
    }

    [Fact]
    public async Task NoHeader_WhenNoHttpContext()
    {
        var sent = await SendAsync(
            new HttpContextAccessor { HttpContext = null },
            new HttpRequestMessage(HttpMethod.Get, "https://deals.test/api/deals"));

        sent.Headers.Authorization.ShouldBeNull();
    }
}
