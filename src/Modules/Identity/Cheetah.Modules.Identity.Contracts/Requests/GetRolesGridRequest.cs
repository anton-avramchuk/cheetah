using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Cheetah.Modules.Identity.Contracts.Response;

namespace Cheetah.Modules.Identity.Contracts.Requests;

[ApiRoute("api/roles", ApiMethod.GetGrid, ResponseType = typeof(RoleViewModel), ServiceName = "Roles")]
public class GetRolesGridRequest : GridRequest;
