using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.VacancyTasks.Contracts.Response;

namespace Crm.VacancyTasks.Contracts.Requests;

[ApiRoute("api/vacancy-tasks", ApiMethod.GetGrid, ResponseType = typeof(VacancyTaskViewModel), ServiceName = "VacancyTasks")]
public class GetAllVacancyTasksRequest : GridRequest;
