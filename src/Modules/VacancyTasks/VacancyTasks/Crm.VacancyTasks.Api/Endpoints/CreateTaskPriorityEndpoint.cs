using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.VacancyTasks.Application.Commands;
using Crm.VacancyTasks.Contracts.Requests;

namespace Crm.VacancyTasks.Api.Endpoints;

public class CreateTaskPriorityEndpoint : CreateCommandEndpoint<CreateTaskPriorityRequest, CreateTaskPriorityCommand>
{
    public override string Route => Constants.TaskPriorityRoute;
    public override string GetByIdRouteName => "GetTaskPriorityById";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("TaskPriorities");
    }
}
