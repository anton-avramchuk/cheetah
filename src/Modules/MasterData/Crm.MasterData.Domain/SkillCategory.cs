using Cheetah.Core.Domain;

namespace Crm.MasterData.Domain;

public class SkillCategory : Entity<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public string Name { get; private set; } = null!;
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    private SkillCategory() { }

    public static SkillCategory Create(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new SkillCategory { Id = Guid.NewGuid(), Name = name };
    }

    public void Update(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }
}
