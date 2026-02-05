using Cheetah.Core.CQRS;

namespace Crm.Recruitment.Application.Queries;

public record GetAllVacanciesQuery : IQuery<IReadOnlyList<VacancyModel>>;
