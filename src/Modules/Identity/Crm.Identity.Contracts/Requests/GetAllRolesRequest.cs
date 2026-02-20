using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.Identity.Contracts.Response;

namespace Crm.Identity.Contracts.Requests;

[ApiRoute("api/roles", ApiMethod.GetCollection, ResponseType = typeof(RoleViewModel), ServiceName = "Roles")]
public record GetAllRolesRequest : ICrmRequest;
