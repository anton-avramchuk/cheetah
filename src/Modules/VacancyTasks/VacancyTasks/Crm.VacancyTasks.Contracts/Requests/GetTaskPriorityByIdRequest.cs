using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.VacancyTasks.Contracts.Response;

namespace Crm.VacancyTasks.Contracts.Requests;

[ApiRoute("api/task-priorities/{id:guid}", ApiMethod.GetOrNotFound, ResponseType = typeof(TaskPriorityViewModel), ServiceName = "TaskPriorities")]
public record GetTaskPriorityByIdRequest([FromRoute] Guid Id) : ICrmRequest;
