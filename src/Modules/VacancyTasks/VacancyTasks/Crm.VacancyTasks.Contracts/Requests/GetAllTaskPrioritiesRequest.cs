using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.VacancyTasks.Contracts.Response;

namespace Crm.VacancyTasks.Contracts.Requests;

[ApiRoute("api/task-priorities", ApiMethod.GetCollection, ResponseType = typeof(TaskPriorityViewModel), ServiceName = "TaskPriorities")]
public record GetAllTaskPrioritiesRequest : ICrmRequest;
