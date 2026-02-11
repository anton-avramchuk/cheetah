using System.Net;
using System.Net.Http.Json;
using Crm.VacancyTasks.Contracts.Requests;
using Crm.VacancyTasks.Contracts.Response;

namespace Crm.VacancyTasks.ApiClient;

internal record CreateEntityResponse(Guid Id);

public class VacancyTasksService : IVacancyTasksService
{
    private const string BasePath = "api/vacancytasks";
    private readonly HttpClient _httpClient;

    public VacancyTasksService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async ValueTask<IReadOnlyList<VacancyTaskViewModel>> GetAllAsync(CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync(BasePath, ct);
        response.EnsureSuccessStatusCode();

        var entities = await response.Content.ReadFromJsonAsync<List<VacancyTaskViewModel>>(ct);
        return entities ?? [];
    }

    public async ValueTask<VacancyTaskViewModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"{BasePath}/{id}", ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<VacancyTaskViewModel>(ct);
    }

    public async ValueTask<Guid> CreateAsync(CreateVacancyTaskRequest request, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath, request, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CreateEntityResponse>(ct);
        return result?.Id ?? throw new InvalidOperationException("Failed to parse response");
    }

    public async ValueTask UpdateAsync(Guid id, UpdateVacancyTaskRequest request, CancellationToken ct = default)
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