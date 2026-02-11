using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.VacancyTasks.Contracts.Requests;

[ApiRoute("api/task-states/{id:guid}", ApiMethod.Delete, ServiceName = "TaskStates")]
public record DeleteTaskStateRequest([FromRoute] Guid Id) : ICrmRequest;
