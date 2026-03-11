using Cheetah.Core.Domain;

namespace Crm.MasterData.Domain;

public class Position : Entity<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public string Name { get; private set; } = null!;
    public Grade Grade { get; private set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    private Position() { }

    public static Position Create(string name, Grade grade)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new Position { Id = Guid.NewGuid(), Name = name, Grade = grade };
    }

    public void Update(string name, Grade grade)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        Grade = grade;
    }
}
