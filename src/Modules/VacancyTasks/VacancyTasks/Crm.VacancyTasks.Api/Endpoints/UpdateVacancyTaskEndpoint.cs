using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.VacancyTasks.Application.Commands;
using Crm.VacancyTasks.Contracts.Requests;

namespace Crm.VacancyTasks.Api.Endpoints;

public class UpdateVacancyTaskEndpoint : UpdateCommandEndpoint<UpdateVacancyTaskRequest, UpdateVacancyTaskCommand>
{
    public override string Route => $"{Constants.DefaultRoute}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("VacancyTasks");
    }
}