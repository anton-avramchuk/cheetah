using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.Recruitment.Application.Commands;
using Crm.Recruitment.Contracts.Requests;

namespace Crm.Recruitment.Api.Endpoints;

public class MoveVacancyEndpoint : PatchCommandEndpoint<MoveVacancyRequest, MoveVacancyCommand>
{
    public override string Route => $"{Constants.VacancyRoute}/{{id:guid}}/move";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("Vacancies");
    }
}
