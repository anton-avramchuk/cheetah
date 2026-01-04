namespace Cheetah.Core.Tenants.Services;

/// <summary>
/// Service for accessing tenant information during database migrations
/// </summary>
public interface ITenantMigrationService
{
    /// <summary>
    /// Gets all active tenants for migration
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of active tenant IDs with their names</returns>
    ValueTask<List<TenantMigrationInfo>> GetAllActiveTenantsAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets connection string for a specific tenant and module
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="moduleName">Module name (e.g., "Identity", "Features")</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Connection string or null if not found</returns>
    ValueTask<string?> GetConnectionStringAsync(Guid tenantId, string moduleName, CancellationToken ct = default);
}

/// <summary>
/// Minimal tenant information for migrations
/// </summary>
public record TenantMigrationInfo(Guid Id, string Name);
