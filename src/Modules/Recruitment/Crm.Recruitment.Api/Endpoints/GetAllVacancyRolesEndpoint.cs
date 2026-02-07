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
}
