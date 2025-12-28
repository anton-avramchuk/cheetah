namespace Cheetah.Tenants.Application.Services;

public interface ICurrentTenant
{
    Guid? Id { get; }
    string? Name { get; }
    bool IsAvailable { get; }

    void SetTenant(Guid? tenantId, string? tenantName = null);
}
