using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.Recruitment.Application.Commands;
using Crm.Recruitment.Contracts.Requests;

namespace Crm.Recruitment.Api.Endpoints;

public class UpdateVacancyStateEndpoint : UpdateCommandEndpoint<UpdateVacancyStateRequest, UpdateVacancyStateCommand>
{
    public override string Route => $"{Constants.VacancyStateRoute}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("VacancyStates");
    }
}
