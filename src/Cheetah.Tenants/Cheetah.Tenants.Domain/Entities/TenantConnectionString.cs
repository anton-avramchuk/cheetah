using Cheetah.Core.Domain;

namespace Cheetah.Tenants.Domain.Entities;

public class TenantConnectionString : Entity<Guid>
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = null!;
    public string ConnectionString { get; private set; } = null!;
    public bool IsDefault { get; private set; }

    private TenantConnectionString() { }

    internal static TenantConnectionString Create(Guid tenantId, string name, string connectionString, bool isDefault)
    {
        return new TenantConnectionString
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            ConnectionString = connectionString,
            IsDefault = isDefault
        };
    }

    internal void UpdateConnectionString(string connectionString)
    {
        ConnectionString = connectionString;
    }

    internal void SetAsDefault()
    {
        IsDefault = true;
    }

    internal void SetAsNonDefault()
    {
        IsDefault = false;
    }
}
