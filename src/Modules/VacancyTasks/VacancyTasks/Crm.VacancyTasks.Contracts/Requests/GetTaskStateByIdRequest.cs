using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.VacancyTasks.Contracts.Response;

namespace Crm.VacancyTasks.Contracts.Requests;

[ApiRoute("api/task-states/{id:guid}", ApiMethod.GetOrNotFound, ResponseType = typeof(TaskStateViewModel), ServiceName = "TaskStates")]
public record GetTaskStateByIdRequest([FromRoute] Guid Id) : ICrmRequest;
