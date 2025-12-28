using Cheetah.Core.DependencyInjection;

namespace Cheetah.Tenants.Application.Services;

[Export(LifetimeType.Scoped)]
public class CurrentTenant : ICurrentTenant
{
    private Guid? _tenantId;
    private string? _tenantName;

    public Guid? Id => _tenantId;
    public string? Name => _tenantName;
    public bool IsAvailable => _tenantId.HasValue;

    public void SetTenant(Guid? tenantId, string? tenantName = null)
    {
        _tenantId = tenantId;
        _tenantName = tenantName;
    }
}
