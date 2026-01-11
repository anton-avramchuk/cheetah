using Cheetah.AspNetCore.Contracts.Responses;

namespace Cheetah.Tenants.Contracts.ViewModels;

public class TenantViewModel : ICrmResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Subdomain { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public List<TenantConnectionStringViewModel> ConnectionStrings { get; set; } = new();
}
