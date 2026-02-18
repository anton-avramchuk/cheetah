using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.Recruitment.Application.Commands;
using Crm.Recruitment.Contracts.Requests;

namespace Crm.Recruitment.Api.Endpoints;

public class CreateVacancyStateEndpoint : CreateCommandEndpoint<CreateVacancyStateRequest, CreateVacancyStateCommand>
{
    public override string Route => Constants.VacancyStateRoute;
    public override string GetByIdRouteName => "GetVacancyStateById";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("VacancyStates");
    }
}
