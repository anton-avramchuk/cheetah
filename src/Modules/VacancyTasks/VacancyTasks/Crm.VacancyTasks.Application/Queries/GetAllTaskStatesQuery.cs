using Cheetah.Core.CQRS;

namespace Crm.VacancyTasks.Application.Queries;

public record GetAllTaskStatesQuery : IQuery<IReadOnlyList<TaskStateModel>>;
