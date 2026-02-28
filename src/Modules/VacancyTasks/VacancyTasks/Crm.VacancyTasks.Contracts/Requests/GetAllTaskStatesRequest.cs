using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.VacancyTasks.Contracts.Response;

namespace Crm.VacancyTasks.Contracts.Requests;

[ApiRoute("api/task-states", ApiMethod.GetGrid, ResponseType = typeof(TaskStateViewModel), ServiceName = "TaskStates")]
public class GetAllTaskStatesRequest : GridRequest;
