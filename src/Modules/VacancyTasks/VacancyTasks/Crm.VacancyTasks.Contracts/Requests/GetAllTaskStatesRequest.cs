using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.VacancyTasks.Contracts.Response;

namespace Crm.VacancyTasks.Contracts.Requests;

[ApiRoute("api/task-states", ApiMethod.GetCollection, ResponseType = typeof(TaskStateViewModel), ServiceName = "TaskStates")]
public record GetAllTaskStatesRequest : ICrmRequest;
