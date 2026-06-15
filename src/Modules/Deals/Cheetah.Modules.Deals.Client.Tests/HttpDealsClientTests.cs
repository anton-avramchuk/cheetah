using System.Net;
using System.Text;
using Cheetah.Modules.Deals.Client;
using Cheetah.Modules.Deals.Contracts;
using Shouldly;

namespace Cheetah.Modules.Deals.Client.Tests;

public class HttpDealsClientTests
{
    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _status;
        private readonly string _json;
        public HttpRequestMessage? LastRequest { get; private set; }

        public StubHandler(HttpStatusCode status, string json = "")
        {
            _status = status;
            _json = json;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new HttpResponseMessage(_status)
            {
                Content = new StringContent(_json, Encoding.UTF8, "application/json")
            });
        }
    }

    private static HttpDealsClient Client(StubHandler handler)
        => new(new HttpClient(handler) { BaseAddress = new Uri("https://deals.test/") });

    [Fact]
    public async Task CreateDealAsync_ParsesReturnedId()
    {
        var id = Guid.NewGuid();
        var handler = new StubHandler(HttpStatusCode.Created, $"{{\"id\":\"{id}\"}}");
        var client = Client(handler);

        var result = await client.CreateDealAsync(
            new CreateDealRequest("Big deal", Guid.NewGuid(), 1000m, "USD", Guid.NewGuid(), Guid.NewGuid()));

        result.ShouldBe(id);
        handler.LastRequest!.Method.ShouldBe(HttpMethod.Post);
        handler.LastRequest.RequestUri!.AbsolutePath.ShouldEndWith("/deals");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_On404()
    {
        var handler = new StubHandler(HttpStatusCode.NotFound);
        var client = Client(handler);

        var result = await client.GetByIdAsync(Guid.NewGuid());

        result.ShouldBeNull();
    }

    [Fact]
    public async Task CreateDealAsync_Throws_OnServerError()
    {
        var handler = new StubHandler(HttpStatusCode.InternalServerError, "boom");
        var client = Client(handler);

        await Should.ThrowAsync<HttpRequestException>(() =>
            client.CreateDealAsync(new CreateDealRequest("X", Guid.NewGuid(), 1m, "USD", Guid.NewGuid(), Guid.NewGuid())).AsTask());
    }
}
