using Cheetah.Core.CQRS;

namespace Crm.VacancyTasks.Application.Queries;

public record GetAllTaskPrioritiesQuery : IQuery<IReadOnlyList<TaskPriorityModel>>;
