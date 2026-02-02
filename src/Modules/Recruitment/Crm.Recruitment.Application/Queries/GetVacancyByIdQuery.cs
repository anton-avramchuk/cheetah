using Cheetah.Core.CQRS;

namespace Crm.Recruitment.Application.Queries;

public record GetVacancyByIdQuery(Guid Id) : IQuery<VacancyModel?>;