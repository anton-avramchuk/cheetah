using Cheetah.Core.Domain;

namespace Crm.MasterData.Domain;

public class StackItem : Entity<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public string Name { get; private set; } = null!;

    public string? Description { get; private set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    private StackItem()
    {
    } // For EF Core

    public static StackItem Create(string name, string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var entity = new StackItem
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description
        };
        return entity;
    }

    public void Update(string name, string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
        Description = description;
    }
}