using System.Net;
using System.Net.Http.Json;
using Crm.Candidates.Contracts.Requests;
using Crm.Candidates.Contracts.Response;

namespace Crm.Candidates.ApiClient;

internal record CreateEntityResponse(Guid Id);

public class CandidatesService : ICandidatesService
{
    private const string BasePath = "api/candidates";
    private readonly HttpClient _httpClient;

    public CandidatesService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async ValueTask<IReadOnlyList<CandidateViewModel>> GetAllAsync(CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync(BasePath, ct);
        response.EnsureSuccessStatusCode();

        var entities = await response.Content.ReadFromJsonAsync<List<CandidateViewModel>>(ct);
        return entities ?? [];
    }

    public async ValueTask<CandidateViewModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"{BasePath}/{id}", ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CandidateViewModel>(ct);
    }

    public async ValueTask<Guid> CreateAsync(CreateCandidateRequest request, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath, request, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CreateEntityResponse>(ct);
        return result?.Id ?? throw new InvalidOperationException("Failed to parse response");
    }

    public async ValueTask UpdateAsync(Guid id, UpdateCandidateRequest request, CancellationToken ct = default)
    {
        var response = await _httpClient.PutAsJsonAsync($"{BasePath}/{id}", request, ct);
        response.EnsureSuccessStatusCode();
    }

    public async ValueTask DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _httpClient.DeleteAsync($"{BasePath}/{id}", ct);
        response.EnsureSuccessStatusCode();
    }
}