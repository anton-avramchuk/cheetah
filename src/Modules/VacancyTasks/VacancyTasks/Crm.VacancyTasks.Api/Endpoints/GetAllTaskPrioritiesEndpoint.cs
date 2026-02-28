using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.VacancyTasks.Application;
using Crm.VacancyTasks.Application.Queries;
using Crm.VacancyTasks.Contracts.Requests;
using Crm.VacancyTasks.Contracts.Response;

namespace Crm.VacancyTasks.Api.Endpoints;

public class GetAllTaskPrioritiesEndpoint : QueryGridEndpoint<GetAllTaskPrioritiesRequest,
    GetAllTaskPrioritiesQuery, TaskPriorityModel, TaskPriorityViewModel>
{
    public override string Route => Constants.TaskPriorityRoute;

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("TaskPriorities");
    }
}
