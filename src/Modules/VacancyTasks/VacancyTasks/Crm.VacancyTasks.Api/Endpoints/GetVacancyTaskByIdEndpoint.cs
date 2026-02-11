using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.VacancyTasks.Application;
using Crm.VacancyTasks.Application.Queries;
using Crm.VacancyTasks.Contracts.Requests;
using Crm.VacancyTasks.Contracts.Response;

namespace Crm.VacancyTasks.Api.Endpoints;

public class GetVacancyTaskByIdEndpoint : QueryOrNotFoundEndpoint<GetVacancyTaskByIdRequest, GetVacancyTaskByIdQuery,
    VacancyTaskModel, VacancyTaskViewModel>
{
    public override string Route => $"{Constants.DefaultRoute}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithName("GetVacancyTaskById");
        config.WithTags("SampleEntities");
    }
}