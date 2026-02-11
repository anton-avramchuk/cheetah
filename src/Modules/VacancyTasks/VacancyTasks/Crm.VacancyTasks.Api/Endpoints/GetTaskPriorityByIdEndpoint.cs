using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.VacancyTasks.Application;
using Crm.VacancyTasks.Application.Queries;
using Crm.VacancyTasks.Contracts.Requests;
using Crm.VacancyTasks.Contracts.Response;

namespace Crm.VacancyTasks.Api.Endpoints;

public class GetTaskPriorityByIdEndpoint : QueryOrNotFoundEndpoint<GetTaskPriorityByIdRequest, GetTaskPriorityByIdQuery,
    TaskPriorityModel, TaskPriorityViewModel>
{
    public override string Route => $"{Constants.TaskPriorityRoute}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithName("GetTaskPriorityById").WithTags("TaskPriorities");
    }
}
