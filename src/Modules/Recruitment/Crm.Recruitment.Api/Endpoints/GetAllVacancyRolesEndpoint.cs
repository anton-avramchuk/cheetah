using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.Recruitment.Application;
using Crm.Recruitment.Application.Queries;
using Crm.Recruitment.Contracts.Requests;
using Crm.Recruitment.Contracts.Response;

namespace Crm.Recruitment.Api.Endpoints;

public class GetAllVacancyRolesEndpoint : QueryCollectionEndpoint<GetAllVacancyRolesRequest,
    GetAllVacancyRolesQuery, VacancyRoleModel, VacancyRoleViewModel>
{
    public override string Route => "api/vacancy-roles";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("Vacancy Roles");
    }
}
