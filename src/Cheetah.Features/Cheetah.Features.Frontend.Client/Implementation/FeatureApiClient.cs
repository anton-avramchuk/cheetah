using System.Net.Http.Json;
using Cheetah.Core.DependencyInjection;
using Cheetah.Features.Frontend.Client.Interfaces;
using Cheetah.Features.Shared.Requests;
using Cheetah.Features.Shared.ViewModels;

namespace Cheetah.Features.Frontend.Client.Implementation;

/// <summary>
/// HTTP client implementation for feature API operations.
/// Used by Blazor WASM to communicate with backend.
/// </summary>
[Export(LifetimeType.Scoped, typeof(IFeatureApiClient))]
public class FeatureApiClient(HttpClient httpClient) : IFeatureApiClient
{
    private const string BaseUrl = "/api/features";

    public async Task<List<FeatureViewModel>> GetAllFeaturesAsync(CancellationToken ct = default)
    {
        var response = await httpClient.GetAsync(BaseUrl, ct);
        response.EnsureSuccessStatusCode();

        var features = await response.Content.ReadFromJsonAsync<List<FeatureViewModel>>(ct);
        return features ?? new List<FeatureViewModel>();
    }

    public async Task<FeatureViewModel?> GetFeatureByIdAsync(string id, CancellationToken ct = default)
    {
        var response = await httpClient.GetAsync($"{BaseUrl}/{id}", ct);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<FeatureViewModel>(ct);
    }

    public async Task<string> CreateFeatureAsync(CreateFeatureRequest request, CancellationToken ct = default)
    {
        var response = await httpClient.PostAsJsonAsync(BaseUrl, request, ct);
        response.EnsureSuccessStatusCode();

        var featureId = await response.Content.ReadFromJsonAsync<string>(ct);
        return featureId ?? string.Empty;
    }

    public async Task UpdateFeatureAsync(string id, UpdateFeatureRequest request, CancellationToken ct = default)
    {
        var response = await httpClient.PutAsJsonAsync($"{BaseUrl}/{id}", request, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<TenantFeatureViewModel>> GetTenantFeaturesAsync(Guid tenantId, CancellationToken ct = default)
    {
        var response = await httpClient.GetAsync($"/api/tenants/{tenantId}/features", ct);
        response.EnsureSuccessStatusCode();

        var features = await response.Content.ReadFromJsonAsync<List<TenantFeatureViewModel>>(ct);
        return features ?? new List<TenantFeatureViewModel>();
    }

    public async Task EnableFeatureAsync(Guid tenantId, string featureId, CancellationToken ct = default)
    {
        var response = await httpClient.PostAsync($"/api/tenants/{tenantId}/features/{featureId}/enable", null, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task DisableFeatureAsync(Guid tenantId, string featureId, CancellationToken ct = default)
    {
        var response = await httpClient.PostAsync($"/api/tenants/{tenantId}/features/{featureId}/disable", null, ct);
        response.EnsureSuccessStatusCode();
    }
}
