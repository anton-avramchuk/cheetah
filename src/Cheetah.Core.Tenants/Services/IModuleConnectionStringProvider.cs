namespace Cheetah.Core.Tenants.Services;

/// <summary>
/// Provides information about module database requirements
/// Modules that need tenant-specific databases should implement and register this interface
/// </summary>
public interface IModuleConnectionStringProvider
{
    /// <summary>
    /// Module name (e.g., "Identity", "Features", "Permissions")
    /// This name is used as the key in TenantConnectionString.Name
    /// </summary>
    string ModuleName { get; }

    /// <summary>
    /// Generates a connection string for a specific tenant
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="tenantName">Tenant name (for database naming)</param>
    /// <param name="baseConnectionString">Base connection string template from configuration</param>
    /// <returns>Tenant-specific connection string</returns>
    string GenerateConnectionString(Guid tenantId, string tenantName, string baseConnectionString);
}