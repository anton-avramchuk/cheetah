using Cheetah.Core.Domain;

namespace Crm.Customer.Domain;

/// <summary>
/// Read-only ACL replica of MasterData.Industry, synchronized by a background task.
/// The Id matches the source Industry.Id in MasterData.
/// </summary>
public class CustomerIndustry : Entity<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public string Name { get; private set; } = null!;

    public DateTimeOffset? CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    private CustomerIndustry() { } // For EF Core

    public static CustomerIndustry Create(Guid id, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new CustomerIndustry { Id = id, Name = name };
    }

    public void Update(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }
}
