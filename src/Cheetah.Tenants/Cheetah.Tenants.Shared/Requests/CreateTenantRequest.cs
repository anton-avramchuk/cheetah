namespace Cheetah.Tenants.Shared.Requests;

public class CreateTenantRequest
{
    public string Name { get; set; } = null!;
    public string? Subdomain { get; set; }
}
