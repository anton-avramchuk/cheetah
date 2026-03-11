using Cheetah.Core.Domain;

namespace Crm.MasterData.Domain;

public class CandidateSource : Entity<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public string Name { get; private set; } = null!;
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    private CandidateSource() { }

    public static CandidateSource Create(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new CandidateSource { Id = Guid.NewGuid(), Name = name };
    }

    public void Update(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }
}
