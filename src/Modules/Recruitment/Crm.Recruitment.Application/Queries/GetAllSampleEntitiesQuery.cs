using Cheetah.Core.CQRS;

namespace Crm.Recruitment.Application.Queries;

public record GetAllSampleEntitiesQuery : IQuery<IReadOnlyList<VacancyModel>>;