using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Cheetah.Modules.Notes.Contracts;
using Cheetah.Modules.Notes.Shared;

namespace Cheetah.Modules.Notes.Client;

/// <summary>
/// Реализация <see cref="INotesClient{TCreateRequest,TUpdateRequest,TDto}"/> поверх
/// <see cref="HttpClient"/>. Маршруты — те же, что объявляют шаблоны эндпоинтов Api-слоя.
/// </summary>
public sealed class HttpNotesClient<TCreateRequest, TUpdateRequest, TDto>
    : INotesClient<TCreateRequest, TUpdateRequest, TDto>
    where TCreateRequest : CreateNoteRequestBase
    where TUpdateRequest : UpdateNoteRequestBase
    where TDto : NoteDtoBase
{
    private static readonly string Base = NotesConstants.DefaultNotesRoutePrefix;

    private readonly HttpClient _http;

    public HttpNotesClient(HttpClient http) => _http = http;

    public async ValueTask<Guid> CreateAsync(TCreateRequest request, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync(Base, request, ct);
        await EnsureSuccessOrThrowAsync(resp, "POST /notes", ct);

        CreatedNote? created = null;
        try { created = await resp.Content.ReadFromJsonAsync<CreatedNote>(ct); }
        // JsonException — тело не разобрать; NotSupportedException — Content-Type не JSON
        // (например, HTML-страница от прокси). В обоих случаях ниже отдаём внятную ошибку.
        catch (Exception ex) when (ex is JsonException or NotSupportedException) { }

        if (created is null || created.Id == Guid.Empty)
            throw new HttpRequestException(
                $"POST /notes returned {(int)resp.StatusCode} without a note id",
                inner: null,
                statusCode: resp.StatusCode);

        return created.Id;
    }

    public async ValueTask<TDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var resp = await _http.GetAsync($"{Base}/{id}", ct);
        if (resp.StatusCode == HttpStatusCode.NotFound)
            return null;

        await EnsureSuccessOrThrowAsync(resp, "GET /notes/{id}", ct);
        return await resp.Content.ReadFromJsonAsync<TDto>(ct);
    }

    public async ValueTask<IReadOnlyList<TDto>> GetByEntityAsync(
        string entityType, Guid entityId, bool pinnedOnly = false, int skip = 0, int take = 0,
        CancellationToken ct = default)
    {
        var url = $"{Base}?entityType={Uri.EscapeDataString(entityType)}&entityId={entityId}"
                  + $"&pinnedOnly={(pinnedOnly ? "true" : "false")}&skip={skip}&take={Page(take)}";

        var resp = await _http.GetAsync(url, ct);
        await EnsureSuccessOrThrowAsync(resp, "GET /notes", ct);

        var body = await resp.Content.ReadFromJsonAsync<List<TDto>>(ct);
        return body ?? new List<TDto>();
    }

    public async ValueTask<IReadOnlyList<TDto>> GetRepliesAsync(
        Guid id, int skip = 0, int take = 0, CancellationToken ct = default)
    {
        var url = $"{Base}/{id}/replies?skip={skip}&take={Page(take)}";
        var resp = await _http.GetAsync(url, ct);
        await EnsureSuccessOrThrowAsync(resp, "GET /notes/{id}/replies", ct);

        var body = await resp.Content.ReadFromJsonAsync<List<TDto>>(ct);
        return body ?? new List<TDto>();
    }

    public async ValueTask UpdateAsync(Guid id, TUpdateRequest request, CancellationToken ct = default)
    {
        var resp = await _http.PutAsJsonAsync($"{Base}/{id}", request, ct);
        await EnsureSuccessOrThrowAsync(resp, "PUT /notes/{id}", ct);
    }

    public ValueTask PinAsync(Guid id, CancellationToken ct = default)
        => PostLifecycleAsync($"{Base}/{id}/pin", "POST /notes/{id}/pin", ct);

    public ValueTask UnpinAsync(Guid id, CancellationToken ct = default)
        => PostLifecycleAsync($"{Base}/{id}/unpin", "POST /notes/{id}/unpin", ct);

    public async ValueTask RemoveAsync(Guid id, CancellationToken ct = default)
    {
        var resp = await _http.DeleteAsync($"{Base}/{id}", ct);
        await EnsureSuccessOrThrowAsync(resp, "DELETE /notes/{id}", ct);
    }

    private async ValueTask PostLifecycleAsync(string url, string op, CancellationToken ct)
    {
        var resp = await _http.PostAsync(url, content: null, ct);
        await EnsureSuccessOrThrowAsync(resp, op, ct);
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

    private static int Page(int take) => take <= 0 ? NotesConstants.DefaultPageSize : take;

    private static string Truncate(string value, int max)
        => value.Length <= max ? value : value[..max] + "…";

    /// <summary>Тело ответа создания — <c>GuidResponse</c> Api-слоя (клиент не зависит от него по сборке).</summary>
    private sealed record CreatedNote(Guid Id);
}
