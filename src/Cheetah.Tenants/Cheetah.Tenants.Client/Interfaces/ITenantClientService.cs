using Cheetah.Tenants.Contracts.ViewModels;

namespace Cheetah.Tenants.Client.Interfaces;

/// <summary>
/// Client service for accessing tenant information.
/// This abstraction allows switching between direct access and HTTP client for microservices.
/// </summary>
public interface ITenantClientService
{
    /// <summary>
    /// Gets a tenant by ID.
    /// </summary>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Tenant view model or null if not found.</returns>
    ValueTask<TenantViewModel?> GetByIdAsync(Guid tenantId, CancellationToken ct = default);

    /// <summary>
    /// Gets a tenant by subdomain.
    /// </summary>
    /// <param name="subdomain">The subdomain.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Tenant view model or null if not found.</returns>
    ValueTask<TenantViewModel?> GetBySubdomainAsync(string subdomain, CancellationToken ct = default);

    /// <summary>
    /// Gets all active tenants.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of active tenants.</returns>
    ValueTask<List<TenantViewModel>> GetAllActiveAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets connection string for a specific tenant.
    /// </summary>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="name">Connection string name (default: "Default").</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Connection string or null if not found.</returns>
    ValueTask<string?> GetConnectionStringAsync(Guid tenantId, string name = "Default", CancellationToken ct = default);

    /// <summary>
    /// Checks if a tenant exists and is active.
    /// </summary>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if tenant exists and is active.</returns>
    ValueTask<bool> IsActiveAsync(Guid tenantId, CancellationToken ct = default);

    /// <summary>
    /// Checks if a tenant exists.
    /// </summary>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if tenant exists.</returns>
    ValueTask<bool> ExistsAsync(Guid tenantId, CancellationToken ct = default);
}
