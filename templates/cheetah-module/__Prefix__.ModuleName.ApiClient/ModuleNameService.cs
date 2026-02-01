using System.Net;
using System.Net.Http.Json;
using __Prefix__.ModuleName.Contracts.Requests;
using __Prefix__.ModuleName.Contracts.Response;

namespace __Prefix__.ModuleName.ApiClient;

internal record CreateEntityResponse(Guid Id);

public class ModuleNameService : IModuleNameService
{
    private const string BasePath = "api/moduleschema";
    private readonly HttpClient _httpClient;

    public ModuleNameService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async ValueTask<IReadOnlyList<SampleEntityViewModel>> GetAllAsync(CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync(BasePath, ct);
        response.EnsureSuccessStatusCode();

        var entities = await response.Content.ReadFromJsonAsync<List<SampleEntityViewModel>>(ct);
        return entities ?? [];
    }

    public async ValueTask<SampleEntityViewModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"{BasePath}/{id}", ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<SampleEntityViewModel>(ct);
    }

    public async ValueTask<Guid> CreateAsync(CreateSampleEntityRequest request, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath, request, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CreateEntityResponse>(ct);
        return result?.Id ?? throw new InvalidOperationException("Failed to parse response");
    }

    public async ValueTask UpdateAsync(Guid id, UpdateSampleEntityRequest request, CancellationToken ct = default)
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
