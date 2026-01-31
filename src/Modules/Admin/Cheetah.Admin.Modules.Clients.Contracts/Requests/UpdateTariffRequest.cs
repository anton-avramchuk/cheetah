using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Cheetah.Admin.Modules.Clients.Contracts.Requests;

public record UpdateTariffRequest(
    [FromRoute] Guid Id,
    [property: Required(AllowEmptyStrings = false)]
    string Name,
    string? Description,
    [property: Range(0, double.MaxValue)]
    decimal Price,
    [property: Required(AllowEmptyStrings = false)]
    [property: StringLength(3, MinimumLength = 3)]
    string Currency,
    bool IsActive) : ICrmRequest;
