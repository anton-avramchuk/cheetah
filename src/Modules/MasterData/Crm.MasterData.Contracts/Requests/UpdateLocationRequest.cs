using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.MasterData.Contracts.Requests;

public record UpdateLocationRequest(
    [FromRoute] Guid Id,
    [property: Required(AllowEmptyStrings = false)]
    string Country,
    [property: Required(AllowEmptyStrings = false)]
    string City,
    [property: Required(AllowEmptyStrings = false)]
    string Timezone) : ICrmRequest;
