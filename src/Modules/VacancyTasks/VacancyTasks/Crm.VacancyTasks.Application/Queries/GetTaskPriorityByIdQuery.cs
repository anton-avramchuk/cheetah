using Cheetah.Core.CQRS;

namespace Crm.VacancyTasks.Application.Queries;

public record GetTaskPriorityByIdQuery(Guid Id) : IQuery<TaskPriorityModel?>;
