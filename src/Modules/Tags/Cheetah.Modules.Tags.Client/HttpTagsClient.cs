using System.Net.Http.Json;
using Cheetah.Modules.Tags.Contracts.Assignments;
using Cheetah.Modules.Tags.Contracts.Registry;
using Cheetah.Modules.Tags.Contracts.Tags;

namespace Cheetah.Modules.Tags.Client;

public sealed class HttpTagsClient : ITagsClient
{
    private const string Base = "api/tags";
    private readonly HttpClient _http;

    public HttpTagsClient(HttpClient http) => _http = http;

    public async ValueTask SyncRegistryAsync(RegistrySyncRequest request, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync($"{Base}/registry/sync", request, ct);
        await EnsureSuccessOrThrowAsync(resp, "POST /registry/sync", ct);
    }

    public async ValueTask AssignAsync(AssignTagsRequest request, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync($"{Base}/assignments", request, ct);
        await EnsureSuccessOrThrowAsync(resp, "POST /assignments", ct);
    }

    public async ValueTask UnassignAsync(UnassignTagsRequest request, CancellationToken ct = default)
    {
        var msg = new HttpRequestMessage(HttpMethod.Delete, $"{Base}/assignments")
        {
            Content = JsonContent.Create(request)
        };
        var resp = await _http.SendAsync(msg, ct);
        await EnsureSuccessOrThrowAsync(resp, "DELETE /assignments", ct);
    }

    public async ValueTask<IReadOnlyList<TagDto>> GetEntityTagsAsync(
        string entityType, Guid entityId, CancellationToken ct = default)
    {
        var url = $"{Base}/assignments?entityType={Uri.EscapeDataString(entityType)}&entityId={entityId}";
        var resp = await _http.GetAsync(url, ct);
        await EnsureSuccessOrThrowAsync(resp, "GET /assignments", ct);
        var body = await resp.Content.ReadFromJsonAsync<List<TagDto>>(ct);
        return body ?? new List<TagDto>();
    }

    private static async ValueTask EnsureSuccessOrThrowAsync(HttpResponseMessage resp, string op, CancellationToken ct)
    {
        if (resp.IsSuccessStatusCode) return;

        var body = "";
        try { body = await resp.Content.ReadAsStringAsync(ct); }
        catch { /* swallow — лучше отдать исходный статус, чем new exception */ }

        throw new HttpRequestException(
            $"{op} failed: {(int)resp.StatusCode} {resp.ReasonPhrase}. Body: {Truncate(body, 500)}",
            inner: null,
            statusCode: resp.StatusCode);
    }

    private static string Truncate(string s, int max)
        => s.Length <= max ? s : s.Substring(0, max) + "…";
}
