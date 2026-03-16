using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Cheetah.Modules.Identity.Contracts.Requests;

[ApiRoute("api/roles", ApiMethod.Create, ServiceName = "Roles")]
public record CreateRoleRequest(
    [property: Required(AllowEmptyStrings = false)]
    string Name) : ICrmRequest;
