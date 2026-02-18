using Cheetah.Core.CQRS;

namespace Crm.Recruitment.Application.Queries;

public record GetAllVacancyStatesQuery : IQuery<IReadOnlyList<VacancyStateModel>>;
