using Cheetah.Core.Domain;

namespace Crm.MasterData.Domain;

public class Industry : Entity<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public string Name { get; private set; } = null!;

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    
    private Industry(){}
    

    public static Industry Create(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Industry
        {
            Id = Guid.NewGuid(),
            Name = name
        };
    }
    
    
    public void ChangeName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        Name = name;
    }
}