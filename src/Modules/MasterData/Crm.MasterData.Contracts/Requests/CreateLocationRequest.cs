using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.MasterData.Contracts.Requests;

[ApiRoute("api/locations", ApiMethod.Create, ServiceName = "Location")]
public record CreateLocationRequest(
    [property: Required(AllowEmptyStrings = false)]
    string Country,
    [property: Required(AllowEmptyStrings = false)]
    string City,
    [property: Required(AllowEmptyStrings = false)]
    string Timezone) : ICrmRequest;
