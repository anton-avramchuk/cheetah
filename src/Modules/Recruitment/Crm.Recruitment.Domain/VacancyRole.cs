using Cheetah.Core.Domain;

namespace Crm.Recruitment.Domain;

/// <summary>
/// Role that can be assigned to a user on a vacancy.
/// </summary>
public class VacancyRole : Entity<Guid>
{
    private readonly List<VacancyAssignment> _assignments = [];

    private VacancyRole()
    {
    }

    public string Name { get; private set; } = null!;

    public string Code { get; private set; } = null!;

    /// <summary>
    /// If true, only one user can be assigned with this role per vacancy.
    /// </summary>
    public bool IsSingle { get; private set; }

    public int Order { get; private set; }

    public IReadOnlyCollection<VacancyAssignment> Assignments => _assignments.AsReadOnly();

    public static VacancyRole Create(string name, string code, bool isSingle = false, int order = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        return new VacancyRole
        {
            Id = Guid.NewGuid(),
            Name = name,
            Code = code.ToLowerInvariant(),
            IsSingle = isSingle,
            Order = order
        };
    }

    public void Update(string name, string code, bool isSingle, int order)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        Name = name;
        Code = code.ToLowerInvariant();
        IsSingle = isSingle;
        Order = order;
    }
}
