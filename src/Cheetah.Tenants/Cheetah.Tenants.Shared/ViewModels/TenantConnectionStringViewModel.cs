namespace Cheetah.Tenants.Shared.ViewModels;

public class TenantConnectionStringViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string ConnectionString { get; set; } = null!;
    public bool IsDefault { get; set; }
}
