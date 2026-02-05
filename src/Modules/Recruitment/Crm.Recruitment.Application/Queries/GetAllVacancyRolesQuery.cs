using Cheetah.Core.CQRS;

namespace Crm.Recruitment.Application.Queries;

public record GetAllVacancyRolesQuery : IQuery<IReadOnlyList<VacancyRoleModel>>;
