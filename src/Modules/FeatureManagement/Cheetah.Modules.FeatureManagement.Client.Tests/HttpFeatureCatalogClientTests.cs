using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Cheetah.FeatureManagement;
using Cheetah.Modules.FeatureManagement.Contracts;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace Cheetah.Modules.FeatureManagement.Client.Tests;

public class HttpFeatureCatalogClientTests
{
    private static HttpClient Client(StubHandler handler)
        => new(handler) { BaseAddress = new Uri("http://catalog.local") };

    [Fact]
    public async Task SyncAsync_posts_descriptors_to_registry()
    {
        var handler = new StubHandler((_, _) => new HttpResponseMessage(HttpStatusCode.NoContent));
        var client = new HttpFeatureCatalogClient(Client(handler));

        await client.SyncAsync(new[] { new FeatureDefinitionDescriptor("deals.kanban-v2", "Kanban", "deals") });

        handler.LastRequest!.Method.ShouldBe(HttpMethod.Post);
        handler.LastRequest.RequestUri!.AbsolutePath.ShouldBe("/api/features/registry/sync");
        handler.LastBody!.ShouldContain("deals.kanban-v2");
    }

    [Fact]
    public async Task EvaluateAsync_parses_result_map()
    {
        var payload = JsonSerializer.Serialize(new Dictionary<string, FeatureEvaluationDto>
        {
            ["a"] = new(true, null),
            ["b"] = new(false, null)
        });
        var handler = new StubHandler((_, _) => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json")
        });
        var client = new HttpFeatureCatalogClient(Client(handler));

        var result = await client.EvaluateAsync(new[] { "a", "b" }, FeatureContext.Empty);

        result["a"].Enabled.ShouldBeTrue();
        result["b"].Enabled.ShouldBeFalse();
    }

    [Fact]
    public async Task PullDefinitionsAsync_parses_definitions()
    {
        var defs = new[]
        {
            new FeatureDefinition("deals.kanban-v2", true, FeatureValueType.Bool,
                new[] { new TargetingRuleDefinition(0, "Percentage", new Dictionary<string, object?> { ["percentage"] = 50 }) },
                Array.Empty<VariantDefinition>())
        };
        var handler = new StubHandler((_, _) => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(defs)
        });
        var client = new HttpFeatureCatalogClient(Client(handler));

        var result = await client.PullDefinitionsAsync();

        result.ShouldHaveSingleItem();
        result[0].Key.ShouldBe("deals.kanban-v2");
        result[0].Rules.ShouldHaveSingleItem();
        handler.LastRequest!.RequestUri!.AbsolutePath.ShouldBe("/api/features/definitions");
    }

    [Fact]
    public async Task Replica_refresh_populates_and_event_triggers_repull()
    {
        var pulls = 0;
        var stub = new StubCatalogClient(() =>
        {
            pulls++;
            return new[] { new FeatureDefinition("a", true, FeatureValueType.Bool, [], []) };
        });
        var replica = new RemoteFeatureDefinitionProvider(stub, NullLogger<RemoteFeatureDefinitionProvider>.Instance);

        (await replica.GetAsync("a", null)).ShouldBeNull();   // до pull — пусто

        await replica.RefreshAsync();
        (await replica.GetAsync("a", null)).ShouldNotBeNull(); // после pull — есть
        pulls.ShouldBe(1);

        await replica.HandleAsync(new DomainEvents.FeatureFlagToggledIntegrationEvent("a", true));
        pulls.ShouldBe(2);                                      // событие → re-pull
    }

    private sealed class StubCatalogClient(Func<IReadOnlyList<FeatureDefinition>> pull) : IFeatureCatalogClient
    {
        public ValueTask SyncAsync(IReadOnlyList<FeatureDefinitionDescriptor> descriptors, CancellationToken ct = default)
            => ValueTask.CompletedTask;
        public ValueTask<IReadOnlyDictionary<string, FeatureEvaluationDto>> EvaluateAsync(
            IReadOnlyList<string> keys, FeatureContext context, CancellationToken ct = default)
            => ValueTask.FromResult<IReadOnlyDictionary<string, FeatureEvaluationDto>>(new Dictionary<string, FeatureEvaluationDto>());
        public ValueTask<IReadOnlyList<FeatureDefinition>> PullDefinitionsAsync(Guid? tenantId = null, CancellationToken ct = default)
            => ValueTask.FromResult(pull());
    }
}
