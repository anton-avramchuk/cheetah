using System.Net;
using System.Net.Http.Json;
using Crm.Customer.Contracts.Requests;
using Crm.Customer.Contracts.Response;

namespace Crm.Customer.ApiClient;

internal record CreateEntityResponse(Guid Id);

public class CustomerService : ICustomerService
{
    private const string BasePath = "api/customer";
    private readonly HttpClient _httpClient;

    public CustomerService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async ValueTask<IReadOnlyList<CustomerViewModel>> GetAllAsync(CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync(BasePath, ct);
        response.EnsureSuccessStatusCode();

        var entities = await response.Content.ReadFromJsonAsync<List<CustomerViewModel>>(ct);
        return entities ?? [];
    }

    public async ValueTask<CustomerViewModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"{BasePath}/{id}", ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CustomerViewModel>(ct);
    }

    public async ValueTask<Guid> CreateAsync(CreateCustomerRequest request, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath, request, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CreateEntityResponse>(ct);
        return result?.Id ?? throw new InvalidOperationException("Failed to parse response");
    }

    public async ValueTask UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken ct = default)
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