using Cheetah.Tenants.Contracts.Requests;
using Cheetah.Tenants.Contracts.ViewModels;

namespace Cheetah.Tenants.ApiClient.Interfaces;

/// <summary>
/// API client for tenant operations.
/// Used by Blazor WASM frontend to communicate with backend API.
/// </summary>
public interface ITenantApiClient
{
    /// <summary>
    /// Gets all tenants.
    /// </summary>
    Task<List<TenantViewModel>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets a tenant by ID.
    /// </summary>
    Task<TenantViewModel?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Creates a new tenant.
    /// </summary>
    Task<Guid> CreateAsync(CreateTenantRequest request, CancellationToken ct = default);

    /// <summary>
    /// Activates a tenant.
    /// </summary>
    Task ActivateAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Deactivates a tenant.
    /// </summary>
    Task DeactivateAsync(Guid id, CancellationToken ct = default);
}
