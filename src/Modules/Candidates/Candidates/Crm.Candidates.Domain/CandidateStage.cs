using Cheetah.Core.Domain;

namespace Crm.Candidates.Domain;

public class CandidateStage : Entity<Guid>
{
    private readonly List<CandidateApplication> _candidateApplications = [];

    private CandidateStage()
    {
    }

    public string Name { get; private set; } = null!;
    public int Order { get; private set; }
    public string? Color { get; private set; }
    public bool IsDefault { get; private set; }
    public IReadOnlyCollection<CandidateApplication> CandidateApplications => _candidateApplications.AsReadOnly();

    public static CandidateStage Create(string name, int order = 0, string? color = null, bool isDefault = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new CandidateStage
        {
            Id = Guid.NewGuid(),
            Name = name,
            Order = order,
            Color = color,
            IsDefault = isDefault
        };
    }

    public void Update(string name, int order, string? color = null, bool isDefault = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
        Order = order;
        Color = color;
        IsDefault = isDefault;
    }
}
