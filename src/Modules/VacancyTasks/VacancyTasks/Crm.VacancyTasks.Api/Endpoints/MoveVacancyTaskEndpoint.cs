using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.VacancyTasks.Application.Commands;
using Crm.VacancyTasks.Contracts.Requests;

namespace Crm.VacancyTasks.Api.Endpoints;

public class MoveVacancyTaskEndpoint : PatchCommandEndpoint<MoveVacancyTaskRequest, MoveVacancyTaskCommand>
{
    public override string Route => $"{Constants.DefaultRoute}/{{id:guid}}/move";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("VacancyTasks");
    }
}
