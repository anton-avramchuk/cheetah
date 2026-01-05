using Cheetah.Features.Contracts.ViewModels;

namespace Cheetah.Features.Client.Interfaces;

/// <summary>
/// Client service for Features module (backend-to-backend communication)
/// Uses IDispatcher for CQRS queries
/// </summary>
public interface IFeatureClientService
{
    /// <summary>
    /// Gets all features
    /// </summary>
    ValueTask<List<FeatureViewModel>> GetAllFeaturesAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets features for a specific tenant
    /// </summary>
    ValueTask<List<TenantFeatureViewModel>> GetTenantFeaturesAsync(Guid tenantId, CancellationToken ct = default);

    /// <summary>
    /// Checks if a feature is enabled for a tenant
    /// </summary>
    ValueTask<bool> IsFeatureEnabledAsync(Guid tenantId, string featureId, CancellationToken ct = default);

    /// <summary>
    /// Enables a feature for a tenant
    /// </summary>
    ValueTask EnableFeatureAsync(Guid tenantId, string featureId, CancellationToken ct = default);

    /// <summary>
    /// Disables a feature for a tenant
    /// </summary>
    ValueTask DisableFeatureAsync(Guid tenantId, string featureId, CancellationToken ct = default);
}
