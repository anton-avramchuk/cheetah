using System.Net;
using System.Text;
using Cheetah.Backend.ServiceAuth;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Cheetah.Backend.ServiceAuth.Tests;

public class CachingServiceTokenProviderTests
{
    private sealed class StubHandler(HttpStatusCode status, string json) : HttpMessageHandler
    {
        public int Calls { get; private set; }
        public string? LastBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            if (request.Content is not null)
                LastBody = await request.Content.ReadAsStringAsync(cancellationToken);

            return new HttpResponseMessage(status)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        }
    }

    private sealed class SingleClientFactory(HttpMessageHandler handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(handler);
    }

    private static CachingServiceTokenProvider Provider(StubHandler handler)
    {
        var options = Options.Create(new ServiceAuthOptions
        {
            TokenEndpoint = "https://identity.test/api/auth/service-token",
            ClientId = "svc-deals",
            ClientSecret = "s3cr3t",
        });

        return new CachingServiceTokenProvider(
            new SingleClientFactory(handler),
            options,
            NullLogger<CachingServiceTokenProvider>.Instance);
    }

    [Fact]
    public async Task GetTokenAsync_FetchesToken_FromEndpoint()
    {
        var handler = new StubHandler(HttpStatusCode.OK,
            """{"accessToken":"abc.def.ghi","tokenType":"Bearer","expiresIn":600}""");
        var provider = Provider(handler);

        var token = await provider.GetTokenAsync();

        token.ShouldBe("abc.def.ghi");
        handler.Calls.ShouldBe(1);
    }

    [Fact]
    public async Task GetTokenAsync_CachesToken_AcrossCalls()
    {
        var handler = new StubHandler(HttpStatusCode.OK,
            """{"accessToken":"abc","tokenType":"Bearer","expiresIn":600}""");
        var provider = Provider(handler);

        await provider.GetTokenAsync();
        await provider.GetTokenAsync();

        handler.Calls.ShouldBe(1); // второй вызов — из кэша
    }

    [Fact]
    public async Task GetTokenAsync_SendsClientCredentials_InBody()
    {
        var handler = new StubHandler(HttpStatusCode.OK,
            """{"accessToken":"abc","tokenType":"Bearer","expiresIn":600}""");
        var provider = Provider(handler);

        await provider.GetTokenAsync();

        handler.LastBody.ShouldContain("svc-deals");
        handler.LastBody.ShouldContain("s3cr3t");
    }

    [Fact]
    public async Task GetTokenAsync_Throws_OnErrorStatus()
    {
        var handler = new StubHandler(HttpStatusCode.Unauthorized, "invalid client");
        var provider = Provider(handler);

        await Should.ThrowAsync<InvalidOperationException>(() => provider.GetTokenAsync().AsTask());
    }
}
