using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.VacancyTasks.Contracts.Requests;

[ApiRoute("api/task-priorities/{id:guid}", ApiMethod.Delete, ServiceName = "TaskPriorities")]
public record DeleteTaskPriorityRequest([FromRoute] Guid Id) : ICrmRequest;
