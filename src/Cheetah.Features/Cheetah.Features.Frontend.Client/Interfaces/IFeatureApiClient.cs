using Cheetah.Features.Shared.Requests;
using Cheetah.Features.Shared.ViewModels;

namespace Cheetah.Features.Frontend.Client.Interfaces;

/// <summary>
/// API client for Features module (frontend-to-backend communication)
/// Used by Blazor WASM to communicate with backend via HTTP
/// </summary>
public interface IFeatureApiClient
{
    /// <summary>
    /// Gets all features
    /// </summary>
    Task<List<FeatureViewModel>> GetAllFeaturesAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets a feature by ID
    /// </summary>
    Task<FeatureViewModel?> GetFeatureByIdAsync(string id, CancellationToken ct = default);

    /// <summary>
    /// Creates a new feature
    /// </summary>
    Task<string> CreateFeatureAsync(CreateFeatureRequest request, CancellationToken ct = default);

    /// <summary>
    /// Updates a feature
    /// </summary>
    Task UpdateFeatureAsync(string id, UpdateFeatureRequest request, CancellationToken ct = default);

    /// <summary>
    /// Gets features for a specific tenant
    /// </summary>
    Task<List<TenantFeatureViewModel>> GetTenantFeaturesAsync(Guid tenantId, CancellationToken ct = default);

    /// <summary>
    /// Enables a feature for a tenant
    /// </summary>
    Task EnableFeatureAsync(Guid tenantId, string featureId, CancellationToken ct = default);

    /// <summary>
    /// Disables a feature for a tenant
    /// </summary>
    Task DisableFeatureAsync(Guid tenantId, string featureId, CancellationToken ct = default);
}
