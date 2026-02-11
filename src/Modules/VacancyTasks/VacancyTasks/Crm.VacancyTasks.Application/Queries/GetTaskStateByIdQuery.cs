using Cheetah.Core.CQRS;

namespace Crm.VacancyTasks.Application.Queries;

public record GetTaskStateByIdQuery(Guid Id) : IQuery<TaskStateModel?>;
