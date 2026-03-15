using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.MasterData.Contracts.Requests;

[ApiRoute("api/industries", ApiMethod.Create, ServiceName = "Industry")]
public record CreateIndustryRequest(
    [property: Required(AllowEmptyStrings = false)]
    string Name) : ICrmRequest;
