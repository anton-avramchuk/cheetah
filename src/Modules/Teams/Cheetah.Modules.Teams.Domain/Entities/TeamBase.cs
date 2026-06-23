using Cheetah.Core.Domain;

namespace Cheetah.Modules.Teams.Domain.Entities;

public abstract class TeamBase : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    private TeamBase()
    {
        
    }


    protected void InitializeCore(Guid id, string name)
    {
        Id = id;
        ChangeName(name);
    }
    
    
    public string Name { get; private set; } = null!;

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }


    public void ChangeName(string name)
    {
        if (Name != name)
        {
            Name = name;
        }
    }
}