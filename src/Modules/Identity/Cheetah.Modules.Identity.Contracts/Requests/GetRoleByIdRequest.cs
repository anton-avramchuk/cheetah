using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Cheetah.Modules.Identity.Contracts.Response;

namespace Cheetah.Modules.Identity.Contracts.Requests;

[ApiRoute("api/roles/{id:guid}", ApiMethod.GetOrNotFound, ResponseType = typeof(RoleViewModel), ServiceName = "Roles")]
public record GetRoleByIdRequest([FromRoute] Guid Id) : ICrmRequest;
