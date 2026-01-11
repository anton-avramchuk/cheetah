using Cheetah.AspNetCore.Contracts.Requests;

namespace Cheetah.Tenants.Contracts.Requests;

public class CreateTenantRequest : ICrmRequest
{
    public string Name { get; set; } = null!;
    public string? Subdomain { get; set; }
}
