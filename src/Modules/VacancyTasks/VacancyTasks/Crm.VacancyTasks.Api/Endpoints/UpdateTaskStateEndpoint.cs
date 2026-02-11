using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.VacancyTasks.Application.Commands;
using Crm.VacancyTasks.Contracts.Requests;

namespace Crm.VacancyTasks.Api.Endpoints;

public class UpdateTaskStateEndpoint : UpdateCommandEndpoint<UpdateTaskStateRequest, UpdateTaskStateCommand>
{
    public override string Route => $"{Constants.TaskStateRoute}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("TaskStates");
    }
}
