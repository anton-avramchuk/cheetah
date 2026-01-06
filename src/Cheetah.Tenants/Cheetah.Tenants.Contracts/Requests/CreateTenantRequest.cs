namespace Cheetah.Tenants.Contracts.Requests;

public class CreateTenantRequest
{
    public string Name { get; set; } = null!;
    public string? Subdomain { get; set; }
}
