using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.VacancyTasks.Application.Commands;
using Crm.VacancyTasks.Contracts.Requests;

namespace Crm.VacancyTasks.Api.Endpoints;

public class CreateVacancyTaskEndpoint : CreateCommandEndpoint<CreateVacancyTaskRequest, CreateVacancyTaskCommand>
{
    public override string Route => Constants.DefaultRoute;

    public override string GetByIdRouteName => "GetVacancyTaskById";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("VacancyTasks");
    }
}