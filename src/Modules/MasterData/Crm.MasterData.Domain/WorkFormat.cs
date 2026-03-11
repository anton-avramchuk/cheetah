using Cheetah.Core.Domain;

namespace Crm.MasterData.Domain;

public class WorkFormat : Entity<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public string Name { get; private set; } = null!;
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    private WorkFormat() { }

    public static WorkFormat Create(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new WorkFormat { Id = Guid.NewGuid(), Name = name };
    }

    public void Update(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }
}
