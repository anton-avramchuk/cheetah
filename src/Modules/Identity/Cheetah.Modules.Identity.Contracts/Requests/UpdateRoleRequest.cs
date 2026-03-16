using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Cheetah.Modules.Identity.Contracts.Requests;

[ApiRoute("api/roles/{id:guid}", ApiMethod.Update, ServiceName = "Roles")]
public record UpdateRoleRequest(
    [FromRoute] Guid Id,
    [property: Required(AllowEmptyStrings = false)]
    string Name) : ICrmRequest;
