using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.VacancyTasks.Application.Commands;
using Crm.VacancyTasks.Contracts.Requests;

namespace Crm.VacancyTasks.Api.Endpoints;

public class CreateTaskStateEndpoint : CreateCommandEndpoint<CreateTaskStateRequest, CreateTaskStateCommand>
{
    public override string Route => Constants.TaskStateRoute;
    public override string GetByIdRouteName => "GetTaskStateById";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("TaskStates");
    }
}
