using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.Identity.Contracts.Requests;

[ApiRoute("api/user-identities/{id:guid}", ApiMethod.Update)]

public record UpdateUserIdentityRequest(
    [FromRoute] Guid Id,
    [property: Required(AllowEmptyStrings = false)]
    string Name,
    string? Description) : ICrmRequest;