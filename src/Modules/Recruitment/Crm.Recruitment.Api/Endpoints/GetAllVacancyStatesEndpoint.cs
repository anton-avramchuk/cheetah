using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.Recruitment.Application;
using Crm.Recruitment.Application.Queries;
using Crm.Recruitment.Contracts.Requests;
using Crm.Recruitment.Contracts.Response;

namespace Crm.Recruitment.Api.Endpoints;

public class GetAllVacancyStatesEndpoint : QueryGridEndpoint<GetAllVacancyStatesRequest,
    GetAllVacancyStatesQuery, VacancyStateModel, VacancyStateViewModel>
{
    public override string Route => Constants.VacancyStateRoute;

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("VacancyStates");
    }
}
