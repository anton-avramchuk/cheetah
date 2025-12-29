using Cheetah.Core.Domain;
using Cheetah.Tenants.Events;

namespace Cheetah.Tenants.Domain.Entities;

public class Tenant : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public string Name { get; private set; } = null!;
    public string NormalizedName { get; private set; } = null!;
    public string? Subdomain { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    private readonly List<TenantConnectionString> _connectionStrings = new();
    public IReadOnlyCollection<TenantConnectionString> ConnectionStrings => _connectionStrings.AsReadOnly();

    private Tenant() { }

    public static Tenant Create(string name, string? subdomain = null)
    {
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = name,
            NormalizedName = name.ToUpperInvariant(),
            Subdomain = subdomain?.ToLowerInvariant(),
            IsActive = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

        tenant.AddDomainEvent(new TenantCreatedEvent(
            tenant.Id,
            tenant.Name,
            tenant.Subdomain,
            DateTime.UtcNow));
        return tenant;
    }

    public void Activate()
    {
        if (IsActive)
            throw new InvalidOperationException($"Tenant {Id} is already active");

        IsActive = true;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new TenantActivatedEvent(Id, DateTime.UtcNow));
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new InvalidOperationException($"Tenant {Id} is already inactive");

        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new TenantDeactivatedEvent(Id, DateTime.UtcNow));
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
