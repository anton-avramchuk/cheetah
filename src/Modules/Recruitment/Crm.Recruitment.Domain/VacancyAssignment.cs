using Cheetah.Core.Domain;

namespace Crm.Recruitment.Domain;

/// <summary>
/// Assignment of a user to a vacancy with a specific role.
/// </summary>
public class VacancyAssignment : Entity<Guid>, ICreateAtEntity
{
    private VacancyAssignment()
    {
    }

    public Guid VacancyId { get; private set; }

    public Vacancy? Vacancy { get; private set; }

    public Guid UserId { get; private set; }

    public User? User { get; private set; }

    public Guid RoleId { get; private set; }

    public VacancyRole? Role { get; private set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public static VacancyAssignment Create(Guid vacancyId, Guid userId, Guid roleId)
    {
        return new VacancyAssignment
        {
            Id = Guid.NewGuid(),
            VacancyId = vacancyId,
            UserId = userId,
            RoleId = roleId
        };
    }
}
