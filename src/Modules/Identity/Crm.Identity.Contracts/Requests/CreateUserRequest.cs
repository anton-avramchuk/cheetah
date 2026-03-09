using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.Identity.Contracts.Requests;

[ApiRoute("api/users", ApiMethod.Create, ServiceName = "Users")]
public record CreateUserRequest(
    [property: Required(AllowEmptyStrings = false)]
    string UserName,
    [property: Required(AllowEmptyStrings = false)]
    [property: EmailAddress]
    string Email,
    [property: Required(AllowEmptyStrings = false)]
    string Password,
    IReadOnlyList<Guid>? RoleIds = null) : ICrmRequest;
