using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Cheetah.Admin.Modules.Clients.Contracts.Requests;

public record UpdateClientRequest(
    [FromRoute] Guid Id,
    [property: Required(AllowEmptyStrings = false)]
    string Name,
    string? Description) : ICrmRequest;
