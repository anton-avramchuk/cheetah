using Cheetah.Core.Domain;
using Cheetah.Core.Tenants.Domain;
using Cheetah.Tenants.Events;

namespace Cheetah.Tenants.Domain.Entities;

public class Tenant : TenantEntity<TenantCreatedEvent, TenantUpdatedEvent, TenantDeactivatedEvent, TenantActivatedEvent>
{
    public string NormalizedName { get; private set; } = null!;
    public string? Subdomain { get; private set; }

    private readonly List<TenantConnectionString> _connectionStrings = new();
    public IReadOnlyCollection<TenantConnectionString> ConnectionStrings => _connectionStrings.AsReadOnly();

    private Tenant() : base()
    { }

    public static Tenant Create(string name, string? subdomain = null)
    {
        var tenant = new Tenant();
        tenant.Id = Guid.NewGuid();
        tenant.Name = name;
        tenant.NormalizedName = name.ToUpperInvariant();
        tenant.Subdomain = subdomain?.ToLowerInvariant();
        tenant.IsActive = false;
        tenant.CreatedAt = DateTimeOffset.UtcNow;

        tenant.AddDomainEvent(tenant.CreateTenantCreatedEvent(tenant.Id, tenant.Name));
        return tenant;
    }

    protected override TenantCreatedEvent CreateTenantCreatedEvent(Guid tenantId, string name)
    {
        return new TenantCreatedEvent(tenantId, name, Subdomain, DateTime.UtcNow);
    }

    protected override TenantUpdatedEvent CreateTenantUpdatedEvent(Guid tenantId, string name)
    {
        return new TenantUpdatedEvent(tenantId, name, IsActive, DateTime.UtcNow);
    }

    protected override TenantDeactivatedEvent CreateTenantDeactivatedEvent(Guid tenantId)
    {
        return new TenantDeactivatedEvent(tenantId, DateTime.UtcNow);
    }

    protected override TenantActivatedEvent CreateTenantActivatedEvent(Guid tenantId)
    {
        return new TenantActivatedEvent(tenantId, DateTime.UtcNow);
    }

    public void Activate()
    {
        if (IsActive)
            throw new InvalidOperationException($"Tenant {Id} is already active");

        IsActive = true;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(CreateTenantActivatedEvent(Id));
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new InvalidOperationException($"Tenant {Id} is already inactive");

        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(CreateTenantDeactivatedEvent(Id));
    }

    public void AddConnectionString(string name, string connectionString, bool isDefault = false)
    {
        if (_connectionStrings.Any(cs => cs.Name == name))
            throw new InvalidOperationException($"Connection string with name '{name}' already exists");

        if (isDefault)
        {
            foreach (var cs in _connectionStrings)
            {
                cs.SetAsNonDefault();
            }
        }

        var tenantConnectionString = TenantConnectionString.Create(Id, name, connectionString, isDefault);
        _connectionStrings.Add(tenantConnectionString);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateConnectionString(string name, string connectionString)
    {
        var existing = _connectionStrings.FirstOrDefault(cs => cs.Name == name);
        if (existing == null)
            throw new InvalidOperationException($"Connection string with name '{name}' not found");

        existing.UpdateConnectionString(connectionString);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetDefaultConnectionString(string name)
    {
        var target = _connectionStrings.FirstOrDefault(cs => cs.Name == name);
        if (target == null)
            throw new InvalidOperationException($"Connection string with name '{name}' not found");

        foreach (var cs in _connectionStrings)
        {
            cs.SetAsNonDefault();
        }

        target.SetAsDefault();
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
