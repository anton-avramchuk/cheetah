using System.Net;
using Cheetah.Workflow;
using Shouldly;

namespace Cheetah.Modules.Workflow.Client.Tests;

public class HttpWorkflowCatalogClientTests
{
    private static HttpClient Client(StubHandler handler)
        => new(handler) { BaseAddress = new Uri("http://workflow.local") };

    [Fact]
    public async Task SyncAsync_posts_triggers_and_actions_to_registry()
    {
        var handler = new StubHandler((_, _) => new HttpResponseMessage(HttpStatusCode.NoContent));
        var client = new HttpWorkflowCatalogClient(Client(handler));

        await client.SyncAsync(
            new[] { new TriggerDescriptor("DealWonIntegrationEvent", "Deals", "Сделка выиграна", new[] { "DealId" }) },
            new[] { new ActionDescriptor("CreateActivity", "Activities", "Создать активность",
                Array.Empty<ActionParameterDescriptor>(), ActionTransport.InProc) });

        handler.LastRequest!.Method.ShouldBe(HttpMethod.Post);
        handler.LastRequest.RequestUri!.AbsolutePath.ShouldBe("/api/automation/registry/sync");
        handler.LastBody!.ShouldContain("DealWonIntegrationEvent");
        handler.LastBody.ShouldContain("CreateActivity");
    }

    [Fact]
    public async Task SyncAsync_throws_on_non_success()
    {
        var handler = new StubHandler((_, _) => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var client = new HttpWorkflowCatalogClient(Client(handler));

        await Should.ThrowAsync<HttpRequestException>(() =>
            client.SyncAsync(
                new[] { new TriggerDescriptor("E", "Svc", "t", Array.Empty<string>()) },
                Array.Empty<ActionDescriptor>()).AsTask());
    }
}
