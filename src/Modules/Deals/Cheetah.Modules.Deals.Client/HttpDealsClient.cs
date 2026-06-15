using System.Net;
using System.Net.Http.Json;
using Cheetah.Modules.Deals.Contracts;

namespace Cheetah.Modules.Deals.Client;

public sealed class HttpDealsClient : IDealsClient
{
    private readonly HttpClient _http;

    public HttpDealsClient(HttpClient http) => _http = http;

    public async ValueTask<Guid> CreateDealAsync(CreateDealRequest request, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync("api/deals", request, ct);
        await EnsureSuccessOrThrowAsync(resp, "POST /deals", ct);
        var body = await resp.Content.ReadFromJsonAsync<CreatedResponse>(ct);
        return body?.Id ?? Guid.Empty;
    }

    public async ValueTask<DealDto?> GetByIdAsync(Guid dealId, CancellationToken ct = default)
    {
        var resp = await _http.GetAsync($"api/deals/{dealId}", ct);
        if (resp.StatusCode == HttpStatusCode.NotFound)
            return null;
        await EnsureSuccessOrThrowAsync(resp, "GET /deals/{id}", ct);
        return await resp.Content.ReadFromJsonAsync<DealDto>(ct);
    }

    private sealed record CreatedResponse(Guid Id);

    private static async ValueTask EnsureSuccessOrThrowAsync(HttpResponseMessage resp, string op, CancellationToken ct)
    {
        if (resp.IsSuccessStatusCode) return;

        var body = "";
        try { body = await resp.Content.ReadAsStringAsync(ct); }
        catch { /* swallow — лучше отдать исходный статус */ }

        throw new HttpRequestException(
            $"{op} failed: {(int)resp.StatusCode} {resp.ReasonPhrase}. Body: {Truncate(body, 500)}",
            inner: null,
            statusCode: resp.StatusCode);
    }

    private static string Truncate(string s, int max)
        => s.Length <= max ? s : s.Substring(0, max) + "…";
}
