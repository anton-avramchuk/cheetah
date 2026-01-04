namespace Cheetah.Core.EntityFramework.Tenants.Configurations;

/// <summary>
/// Configuration options for TenantEntity mapping
/// </summary>
public class TenantEntityConfigurationOptions
{
    /// <summary>
    /// Table name for tenants. Default: "Tenants"
    /// </summary>
    public string TableName { get; set; } = "Tenants";

    /// <summary>
    /// Maximum length for Name field. Default: 256
    /// </summary>
    public int NameMaxLength { get; set; } = 256;

    /// <summary>
    /// Maximum length for Description field. Default: 2000
    /// </summary>
    public int DescriptionMaxLength { get; set; } = 2000;

    /// <summary>
    /// Name of the unique index for Name field. Default: "IX_Tenants_Name"
    /// </summary>
    public string NameIndexName { get; set; } = "IX_Tenants_Name";

    /// <summary>
    /// Name of the index for IsActive field. Default: "IX_Tenants_IsActive"
    /// </summary>
    public string IsActiveIndexName { get; set; } = "IX_Tenants_IsActive";

    /// <summary>
    /// Name of the index for CreatedAt field. Default: "IX_Tenants_CreatedAt"
    /// </summary>
    public string CreatedAtIndexName { get; set; } = "IX_Tenants_CreatedAt";

    /// <summary>
    /// Create unique index on Name field. Default: true
    /// </summary>
    public bool CreateNameUniqueIndex { get; set; } = true;

    /// <summary>
    /// Create index on IsActive field. Default: true
    /// </summary>
    public bool CreateIsActiveIndex { get; set; } = true;

    /// <summary>
    /// Create index on CreatedAt field. Default: true
    /// </summary>
    public bool CreateCreatedAtIndex { get; set; } = true;
}
