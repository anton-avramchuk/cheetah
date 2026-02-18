using Cheetah.Core.CQRS;

namespace Crm.Recruitment.Application.Queries;

public record GetVacancyStateByIdQuery(Guid Id) : IQuery<VacancyStateModel?>;
