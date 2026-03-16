using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Cheetah.Modules.Identity.Contracts.Requests;

[ApiRoute("api/users/{id:guid}", ApiMethod.Update, ServiceName = "Users")]
public record UpdateUserRequest(
    [FromRoute] Guid Id,
    [property: Required(AllowEmptyStrings = false)]
    string UserName,
    [property: Required(AllowEmptyStrings = false)]
    [property: EmailAddress]
    string Email,
    IReadOnlyList<Guid>? RoleIds = null) : ICrmRequest;
