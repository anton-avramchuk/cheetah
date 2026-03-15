using Cheetah.Core.Domain;

namespace Crm.Customer.Domain;

/// <summary>
/// Read-only ACL replica of MasterData.Industry, synchronized by a background task.
/// The Id matches the source Industry.Id in MasterData.
/// ContentHash is the SHA256 of the synced content fields — used for change detection.
/// </summary>
public class CustomerIndustry : Entity<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public string Name { get; private set; } = null!;

    /// <summary>SHA256 hex of the content fields at the time of last sync.</summary>
    public string ContentHash { get; private set; } = string.Empty;

    public DateTimeOffset? CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    private CustomerIndustry() { } // For EF Core

    public static CustomerIndustry Create(Guid id, string name, string contentHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new CustomerIndustry { Id = id, Name = name, ContentHash = contentHash };
    }

    public void Update(string name, string contentHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        ContentHash = contentHash;
    }
}
