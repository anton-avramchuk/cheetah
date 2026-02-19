using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.Identity.Contracts.Requests;

[ApiRoute("api/user-identities", ApiMethod.Create)]
public record CreateUserIdentityRequest(
    [property: Required(AllowEmptyStrings = false)]
    string Name,
    string? Description) : ICrmRequest;