using System.Net.Http.Json;
using Cheetah.Modules.Calendar.Contracts;

namespace Cheetah.Modules.Calendar.Client;

public sealed class HttpCalendarClient : ICalendarClient
{
    private readonly HttpClient _http;

    public HttpCalendarClient(HttpClient http) => _http = http;

    public async ValueTask SyncRegistryAsync(CalendarRegistrySyncRequest request, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync("api/calendar/registry/sync", request, ct);
        await EnsureSuccessOrThrowAsync(resp, "POST /calendar/registry/sync", ct);
    }

    public async ValueTask<Guid> CreateEventAsync(Guid calendarId, CreateEventRequest request, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync($"api/calendars/{calendarId}/events", request, ct);
        await EnsureSuccessOrThrowAsync(resp, "POST /calendars/{id}/events", ct);
        var body = await resp.Content.ReadFromJsonAsync<CreatedResponse>(ct);
        return body?.Id ?? Guid.Empty;
    }

    public async ValueTask<IReadOnlyList<EventOccurrenceDto>> GetByEntityAsync(
        string entityType, Guid entityId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default)
    {
        var url = $"api/calendar-events/by-entity/{Uri.EscapeDataString(entityType)}/{entityId}"
                  + $"?from={Uri.EscapeDataString(from.UtcDateTime.ToString("O"))}"
                  + $"&to={Uri.EscapeDataString(to.UtcDateTime.ToString("O"))}";
        var resp = await _http.GetAsync(url, ct);
        await EnsureSuccessOrThrowAsync(resp, "GET /calendar-events/by-entity", ct);
        var body = await resp.Content.ReadFromJsonAsync<List<EventOccurrenceDto>>(ct);
        return body ?? new List<EventOccurrenceDto>();
    }

    public async ValueTask<IReadOnlyList<BusyIntervalDto>> GetUserBusyAsync(
        Guid hostUserId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default)
    {
        var url = $"api/calendar/users/{hostUserId}/busy"
                  + $"?from={Uri.EscapeDataString(from.UtcDateTime.ToString("O"))}"
                  + $"&to={Uri.EscapeDataString(to.UtcDateTime.ToString("O"))}";
        var resp = await _http.GetAsync(url, ct);
        await EnsureSuccessOrThrowAsync(resp, "GET /calendar/users/{id}/busy", ct);
        var body = await resp.Content.ReadFromJsonAsync<List<BusyIntervalDto>>(ct);
        return body ?? new List<BusyIntervalDto>();
    }

    public async ValueTask RescheduleEventAsync(
        Guid eventId, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default)
    {
        var request = new RescheduleEventRequest(eventId, startUtc.UtcDateTime, endUtc.UtcDateTime);
        var resp = await _http.PostAsJsonAsync($"api/calendar-events/{eventId}/reschedule", request, ct);
        await EnsureSuccessOrThrowAsync(resp, "POST /calendar-events/{id}/reschedule", ct);
    }

    public async ValueTask CancelEventAsync(Guid eventId, CancellationToken ct = default)
    {
        var resp = await _http.DeleteAsync($"api/calendar-events/{eventId}", ct);
        await EnsureSuccessOrThrowAsync(resp, "DELETE /calendar-events/{id}", ct);
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
