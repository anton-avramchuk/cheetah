namespace Cheetah.Features.Application.Services;

/// <summary>
/// Service for checking if features are enabled for tenants
/// </summary>
public interface IFeatureChecker
{
    /// <summary>
    /// Checks if a feature is enabled for a specific tenant
    /// </summary>
    ValueTask<bool> IsEnabledAsync(string featureId, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Requires that a feature is enabled for a specific tenant, throws if not
    /// </summary>
    ValueTask RequireAsync(string featureId, Guid tenantId, CancellationToken cancellationToken = default);
}
