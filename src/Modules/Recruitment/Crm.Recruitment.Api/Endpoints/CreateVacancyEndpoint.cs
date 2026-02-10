using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.Recruitment.Application.Commands;
using Crm.Recruitment.Contracts.Requests;

namespace Crm.Recruitment.Api.Endpoints;

public class CreateVacancyEndpoint : CreateCommandEndpoint<CreateVacancyRequest, CreateVacancyCommand>
{
    public override string Route => Constants.VacancyRoute;

    public override string GetByIdRouteName => "GetVacancyById";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("Vacancies");
    }
}
