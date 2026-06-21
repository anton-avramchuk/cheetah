using System.Net.Http.Json;
using Cheetah.Modules.CustomFields.Contracts;

namespace Cheetah.Modules.CustomFields.Client;

/// <summary>HTTP-реализация <see cref="ICustomFieldsClient"/> поверх эндпоинтов <c>api/custom-fields</c>.</summary>
public sealed class HttpCustomFieldsClient : ICustomFieldsClient
{
    private const string Base = "api/custom-fields";
    private readonly HttpClient _http;

    public HttpCustomFieldsClient(HttpClient http) => _http = http;

    public async ValueTask SyncTypesAsync(
        IReadOnlyList<CustomFieldEntityTypeDescriptor> descriptors, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync($"{Base}/registry/sync", new { descriptors }, ct);
        resp.EnsureSuccessStatusCode();
    }

    public async ValueTask<IReadOnlyList<CustomFieldDefinitionDto>> ListDefinitionsAsync(
        string entityType, bool onlyActive = true, CancellationToken ct = default)
    {
        var resp = await _http.GetAsync(
            $"{Base}/definitions?entityType={Uri.EscapeDataString(entityType)}&onlyActive={onlyActive}", ct);
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadFromJsonAsync<List<CustomFieldDefinitionDto>>(ct);
        return body ?? [];
    }

    public async ValueTask<CustomFieldValuesDto> GetValuesAsync(
        string entityType, string entityId, CancellationToken ct = default)
    {
        var resp = await _http.GetAsync(
            $"{Base}/values?entityType={Uri.EscapeDataString(entityType)}&entityId={Uri.EscapeDataString(entityId)}", ct);
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadFromJsonAsync<CustomFieldValuesDto>(ct);
        return body ?? new CustomFieldValuesDto(entityType, entityId, new Dictionary<string, object?>());
    }

    public async ValueTask<IReadOnlyDictionary<string, IReadOnlyDictionary<string, object?>>> BatchGetValuesAsync(
        string entityType, IReadOnlyList<string> entityIds, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync($"{Base}/values/batch-get",
            new BatchGetValuesRequest(entityType, entityIds), ct);
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content
            .ReadFromJsonAsync<Dictionary<string, IReadOnlyDictionary<string, object?>>>(ct);
        return body ?? new Dictionary<string, IReadOnlyDictionary<string, object?>>();
    }

    public async ValueTask SetValuesAsync(SetCustomFieldValuesRequest request, CancellationToken ct = default)
    {
        var resp = await _http.PutAsJsonAsync($"{Base}/values", request, ct);
        resp.EnsureSuccessStatusCode();
    }
}
