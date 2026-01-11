namespace Cheetah.Core.Tenants.Services;

/// <summary>
/// Default implementation that generates connection strings with pattern: tenant_{tenantId}_{moduleName}
/// </summary>
public sealed class DefaultModuleConnectionStringProvider(string moduleName) : IModuleConnectionStringProvider
{
    public string ModuleName { get; } = moduleName;

    public string GenerateConnectionString(Guid tenantId, string tenantName, string baseConnectionString)
    {
        // Parse base connection string and replace database name
        var builder = new System.Data.Common.DbConnectionStringBuilder
        {
            ConnectionString = baseConnectionString
        };

        // Generate database name: tenant_{tenantId}_{moduleName_lowercase}
        var dbName = $"tenant_{tenantName}_{ModuleName.ToLowerInvariant()}";

        builder["Database"] = dbName;

        return builder.ConnectionString;
    }
}