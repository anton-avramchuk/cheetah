using Cheetah.Core.Attributes;
using Cheetah.Core.Domain;

namespace Crm.Customer.Domain;

/// <summary>
/// Read-only ACL replica of MasterData.Industry, synchronized by a background task.
/// The Id matches the source Industry.Id in MasterData.
/// </summary>
public class CustomerIndustry : Entity<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    [SyncHash]
    public string Name { get; private set; } = null!;

    public string ContentHash { get; private set; } = string.Empty;

    public DateTimeOffset? CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    private CustomerIndustry() { } // For EF Core

    public static CustomerIndustry Create(Guid id, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var entity = new CustomerIndustry { Id = id, Name = name };
        entity.ContentHash = entity.ComputeHash();
        return entity;
    }

    public void Update(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        ContentHash = this.ComputeHash();
    }
}
