namespace Cheetah.Core.Tenants.Services;

/// <summary>
/// Service for managing tenant connection strings
/// </summary>
public interface ITenantConnectionStringService
{
    /// <summary>
    /// Generates connection strings for all registered modules for a new tenant
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="tenantName">Tenant name</param>
    /// <returns>Dictionary of module name -> connection string</returns>
    Task<Dictionary<string, string>> GenerateAllConnectionStringsAsync(Guid tenantId, string tenantName);
}
