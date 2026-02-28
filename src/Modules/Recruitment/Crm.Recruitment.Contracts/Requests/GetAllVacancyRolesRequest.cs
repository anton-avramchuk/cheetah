using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.Recruitment.Contracts.Response;

namespace Crm.Recruitment.Contracts.Requests;

[ApiRoute("api/vacancy-roles", ApiMethod.GetGrid, ResponseType = typeof(VacancyRoleViewModel), ServiceName = "VacancyRoles")]
public class GetAllVacancyRolesRequest : GridRequest;
