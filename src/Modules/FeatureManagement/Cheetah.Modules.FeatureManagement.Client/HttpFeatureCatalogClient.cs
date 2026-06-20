using System.Net.Http.Json;
using Cheetah.FeatureManagement;
using Cheetah.Modules.FeatureManagement.Contracts;

namespace Cheetah.Modules.FeatureManagement.Client;

/// <summary>HTTP-реализация <see cref="IFeatureCatalogClient"/> поверх эндпоинтов <c>api/features</c>.</summary>
public sealed class HttpFeatureCatalogClient : IFeatureCatalogClient
{
    private const string Base = "api/features";
    private readonly HttpClient _http;

    public HttpFeatureCatalogClient(HttpClient http) => _http = http;

    public async ValueTask SyncAsync(IReadOnlyList<FeatureDefinitionDescriptor> descriptors, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync($"{Base}/registry/sync", new { descriptors }, ct);
        resp.EnsureSuccessStatusCode();
    }

    public async ValueTask<IReadOnlyDictionary<string, FeatureEvaluationDto>> EvaluateAsync(
        IReadOnlyList<string> keys, FeatureContext context, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync($"{Base}/evaluate", new { keys, context }, ct);
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadFromJsonAsync<Dictionary<string, FeatureEvaluationDto>>(ct);
        return body ?? new Dictionary<string, FeatureEvaluationDto>();
    }

    public async ValueTask<IReadOnlyList<FeatureDefinition>> PullDefinitionsAsync(Guid? tenantId = null, CancellationToken ct = default)
    {
        var url = $"{Base}/definitions" + (tenantId is { } t ? $"?tenantId={t}" : string.Empty);
        var resp = await _http.GetAsync(url, ct);
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadFromJsonAsync<List<FeatureDefinition>>(ct);
        return body ?? new List<FeatureDefinition>();
    }
}
