using System.Net.Http.Json;

namespace Cheetah.Permissions.Catalog.Client;

public sealed class HttpPermissionsCatalogClient : IPermissionsCatalogClient
{
    private const string Base = "api/permissions/catalog";
    private readonly HttpClient _http;

    public HttpPermissionsCatalogClient(HttpClient http) => _http = http;

    public async ValueTask SyncAsync(RegistrySyncRequest request, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync($"{Base}/sync", request, ct);
        await EnsureSuccessOrThrowAsync(resp, "POST /sync", ct);
    }

    public async ValueTask<IReadOnlyList<PermissionDefinitionDto>> ListAsync(string? module = null, CancellationToken ct = default)
    {
        var url = Base + (string.IsNullOrEmpty(module) ? "" : "?module=" + Uri.EscapeDataString(module));
        var resp = await _http.GetAsync(url, ct);
        await EnsureSuccessOrThrowAsync(resp, "GET /catalog", ct);
        var body = await resp.Content.ReadFromJsonAsync<List<PermissionDefinitionDto>>(ct);
        return body ?? new List<PermissionDefinitionDto>();
    }

    /// <summary>
    /// Заменяет голый EnsureSuccessStatusCode: при ошибке читает тело ответа и кладёт
    /// в сообщение исключения для диагностики (это admin-канал, не hot path).
    /// </summary>
    private static async ValueTask EnsureSuccessOrThrowAsync(HttpResponseMessage resp, string op, CancellationToken ct)
    {
        if (resp.IsSuccessStatusCode) return;

        var body = "";
        try { body = await resp.Content.ReadAsStringAsync(ct); }
        catch { /* swallow — лучше дать оригинальное сообщение, чем new exception */ }

        throw new HttpRequestException(
            $"{op} failed: {(int)resp.StatusCode} {resp.ReasonPhrase}. Body: {Truncate(body, 500)}",
            inner: null,
            statusCode: resp.StatusCode);
    }

    private static string Truncate(string s, int max)
        => s.Length <= max ? s : s.Substring(0, max) + "…";
}
