using Cheetah.Core.Domain;

namespace Cheetah.Modules.Teams.Domain.Entities;

public abstract class TeamMember : Entity<Guid>
{
    private TeamMember()
    {
    }


    protected void InitializeCore(Guid id, string name)
    {
        Id = id;
        ChangeName(name);
    }

    private void ChangeName(string name)
    {
        if (Name != name)
        {
            Name = name;
        }
    }

    public string Name { get; private set; } = null!;
}