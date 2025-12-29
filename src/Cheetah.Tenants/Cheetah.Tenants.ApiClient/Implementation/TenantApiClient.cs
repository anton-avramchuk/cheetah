using System.Net.Http.Json;
using Cheetah.Core.DependencyInjection;
using Cheetah.Tenants.ApiClient.Interfaces;
using Cheetah.Tenants.Shared.Requests;
using Cheetah.Tenants.Shared.ViewModels;

namespace Cheetah.Tenants.ApiClient.Implementation;

/// <summary>
/// HTTP client implementation for tenant API operations.
/// Used by Blazor WASM to communicate with backend.
/// </summary>
[Export(LifetimeType.Scoped, typeof(ITenantApiClient))]
public class TenantApiClient : ITenantApiClient
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "/api/tenants";

    public TenantApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<TenantViewModel>> GetAllAsync(CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync(BaseUrl, ct);
        response.EnsureSuccessStatusCode();

        var tenants = await response.Content.ReadFromJsonAsync<List<TenantViewModel>>(ct);
        return tenants ?? new List<TenantViewModel>();
    }

    public async Task<TenantViewModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"{BaseUrl}/{id}", ct);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TenantViewModel>(ct);
    }

    public async Task<Guid> CreateAsync(CreateTenantRequest request, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BaseUrl, request, ct);
        response.EnsureSuccessStatusCode();

        var tenantId = await response.Content.ReadFromJsonAsync<Guid>(ct);
        return tenantId;
    }

    public async Task ActivateAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsync($"{BaseUrl}/{id}/activate", null, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeactivateAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsync($"{BaseUrl}/{id}/deactivate", null, ct);
        response.EnsureSuccessStatusCode();
    }
}
