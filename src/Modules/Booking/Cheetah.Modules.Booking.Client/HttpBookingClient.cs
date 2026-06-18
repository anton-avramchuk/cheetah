using System.Net;
using System.Net.Http.Json;
using Cheetah.Modules.Booking.Contracts;

namespace Cheetah.Modules.Booking.Client;

public sealed class HttpBookingClient : IBookingClient
{
    private const string PublicBase = "api/public/booking";
    private readonly HttpClient _http;

    public HttpBookingClient(HttpClient http) => _http = http;

    public async ValueTask<PublicBookingPageDto?> GetPageAsync(string slug, CancellationToken ct = default)
    {
        var resp = await _http.GetAsync($"{PublicBase}/{Uri.EscapeDataString(slug)}", ct);
        if (resp.StatusCode == HttpStatusCode.NotFound)
            return null;
        await EnsureSuccessOrThrowAsync(resp, "GET /public/booking/{slug}", ct);
        return await resp.Content.ReadFromJsonAsync<PublicBookingPageDto>(ct);
    }

    public async ValueTask<IReadOnlyList<SlotDto>> GetSlotsAsync(
        string slug, DateOnly from, DateOnly to, string inviteeTimeZone, CancellationToken ct = default)
    {
        var url = $"{PublicBase}/{Uri.EscapeDataString(slug)}/slots"
                  + $"?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}&tz={Uri.EscapeDataString(inviteeTimeZone)}";
        var resp = await _http.GetAsync(url, ct);
        await EnsureSuccessOrThrowAsync(resp, "GET /public/booking/{slug}/slots", ct);
        var body = await resp.Content.ReadFromJsonAsync<List<SlotDto>>(ct);
        return body ?? new List<SlotDto>();
    }

    public async ValueTask<Guid> CreateBookingAsync(string slug, CreatePublicBookingRequest request, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync($"{PublicBase}/{Uri.EscapeDataString(slug)}", request, ct);
        await EnsureSuccessOrThrowAsync(resp, "POST /public/booking/{slug}", ct);
        return await resp.Content.ReadFromJsonAsync<Guid>(ct);
    }

    public async ValueTask RescheduleAsync(string manageToken, DateTimeOffset newStartUtc, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync(
            $"{PublicBase}/manage/{Uri.EscapeDataString(manageToken)}/reschedule",
            new RescheduleBookingRequest(newStartUtc), ct);
        await EnsureSuccessOrThrowAsync(resp, "POST /public/booking/manage/{token}/reschedule", ct);
    }

    public async ValueTask CancelAsync(string manageToken, string reason, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync(
            $"{PublicBase}/manage/{Uri.EscapeDataString(manageToken)}/cancel",
            new CancelBookingRequest(reason), ct);
        await EnsureSuccessOrThrowAsync(resp, "POST /public/booking/manage/{token}/cancel", ct);
    }

    private static async ValueTask EnsureSuccessOrThrowAsync(HttpResponseMessage resp, string op, CancellationToken ct)
    {
        if (resp.IsSuccessStatusCode)
            return;

        var body = "";
        try { body = await resp.Content.ReadAsStringAsync(ct); }
        catch { /* swallow — лучше отдать исходный статус */ }

        throw new HttpRequestException(
            $"{op} failed: {(int)resp.StatusCode} {resp.ReasonPhrase}. Body: {Truncate(body, 500)}",
            inner: null,
            statusCode: resp.StatusCode);
    }

    private static string Truncate(string s, int max) => s.Length <= max ? s : s.Substring(0, max) + "…";
}
