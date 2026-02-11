using Cheetah.Core.CQRS;

namespace Crm.VacancyTasks.Application.Queries;

public record GetVacancyTaskByIdQuery(Guid Id) : IQuery<VacancyTaskModel?>;