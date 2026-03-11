using System.Net;
using System.Net.Http.Json;
using Crm.MasterData.Contracts.Requests;
using Crm.MasterData.Contracts.Response;

namespace Crm.MasterData.ApiClient;

internal record CreateEntityResponse(Guid Id);

public class MasterDataService : IMasterDataService
{
    private const string BasePath = "api/masterdata";
    private readonly HttpClient _httpClient;

    public MasterDataService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async ValueTask<IReadOnlyList<StackItemViewModel>> GetAllAsync(CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync(BasePath, ct);
        response.EnsureSuccessStatusCode();

        var entities = await response.Content.ReadFromJsonAsync<List<StackItemViewModel>>(ct);
        return entities ?? [];
    }

    public async ValueTask<StackItemViewModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"{BasePath}/{id}", ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<StackItemViewModel>(ct);
    }

    public async ValueTask<Guid> CreateAsync(CreateStackItemRequest request, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath, request, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CreateEntityResponse>(ct);
        return result?.Id ?? throw new InvalidOperationException("Failed to parse response");
    }

    public async ValueTask UpdateAsync(Guid id, UpdateStackItemRequest request, CancellationToken ct = default)
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