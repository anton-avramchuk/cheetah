using Cheetah.Core.Domain;
using Cheetah.Core.Domain.ValueObjects;

namespace Crm.Candidates.Domain;

public class CandidateSource : Entity<Guid>
{
    private CandidateSource()
    {
    }

    public string Name { get; private set; } = null!;
    public int Order { get; private set; }
    public Color? Color { get; private set; }

    public static CandidateSource Create(string name, int order = 0, string? color = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new CandidateSource
        {
            Id = Guid.NewGuid(),
            Name = name,
            Order = order,
            Color = color is not null ? Color.Create(color) : null
        };
    }

    public void Update(string name, int order, string? color = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
        Order = order;
        Color = color is not null ? Color.Create(color) : null;
    }
}
