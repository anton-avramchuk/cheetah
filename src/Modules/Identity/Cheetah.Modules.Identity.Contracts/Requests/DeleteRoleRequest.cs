using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Cheetah.Modules.Identity.Contracts.Requests;

[ApiRoute("api/roles/{id:guid}", ApiMethod.Delete, ServiceName = "Roles")]
public record DeleteRoleRequest([FromRoute] Guid Id) : ICrmRequest;
