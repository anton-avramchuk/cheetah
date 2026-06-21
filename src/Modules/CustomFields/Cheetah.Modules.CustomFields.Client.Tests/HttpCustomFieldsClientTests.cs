using System.Net;
using System.Text;
using Cheetah.Modules.CustomFields.Client;
using Cheetah.Modules.CustomFields.Contracts;
using Cheetah.Modules.CustomFields.Shared;
using Shouldly;

namespace Cheetah.Modules.CustomFields.Client.Tests;

public class HttpCustomFieldsClientTests
{
    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _status;
        private readonly string _json;
        public HttpRequestMessage? LastRequest { get; private set; }
        public string? LastBody { get; private set; }

        public StubHandler(HttpStatusCode status, string json = "{}")
        {
            _status = status;
            _json = json;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            if (request.Content is not null)
                LastBody = await request.Content.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(_status)
            {
                Content = new StringContent(_json, Encoding.UTF8, "application/json")
            };
        }
    }

    private static HttpClient Client(StubHandler handler)
        => new(handler) { BaseAddress = new Uri("http://localhost") };

    [Fact]
    public async Task SyncTypes_PostsToRegistrySync()
    {
        var handler = new StubHandler(HttpStatusCode.NoContent);
        var client = new HttpCustomFieldsClient(Client(handler));

        await client.SyncTypesAsync([
            new CustomFieldEntityTypeDescriptor("crm.deal", "Сделка", "deals", CustomFieldEntityIdType.Guid)
        ]);

        handler.LastRequest!.Method.ShouldBe(HttpMethod.Post);
        handler.LastRequest!.RequestUri!.AbsolutePath.ShouldBe("/api/custom-fields/registry/sync");
        handler.LastBody.ShouldContain("crm.deal");
    }

    [Fact]
    public async Task GetValues_DeserializesValues()
    {
        const string json = """{"entityType":"crm.deal","entityId":"d1","values":{"priority":"high"}}""";
        var handler = new StubHandler(HttpStatusCode.OK, json);
        var client = new HttpCustomFieldsClient(Client(handler));

        var dto = await client.GetValuesAsync("crm.deal", "d1");

        dto.EntityId.ShouldBe("d1");
        dto.Values.ShouldContainKey("priority");
    }

    [Fact]
    public async Task SetValues_NonSuccess_Throws()
    {
        var handler = new StubHandler(HttpStatusCode.BadRequest, """{"title":"Validation Error"}""");
        var client = new HttpCustomFieldsClient(Client(handler));

        await Should.ThrowAsync<HttpRequestException>(() =>
            client.SetValuesAsync(new SetCustomFieldValuesRequest("crm.deal", "d1",
                new Dictionary<string, object?> { ["priority"] = "high" })).AsTask());
    }
}
