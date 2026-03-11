using Cheetah.Core.Domain;

namespace Crm.MasterData.Domain;

public class Skill : Entity<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public string Name { get; private set; } = null!;
    public Guid? SkillCategoryId { get; private set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    private Skill() { }

    public static Skill Create(string name, Guid? skillCategoryId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new Skill { Id = Guid.NewGuid(), Name = name, SkillCategoryId = skillCategoryId };
    }

    public void Update(string name, Guid? skillCategoryId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        SkillCategoryId = skillCategoryId;
    }
}
