using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.Recruitment.Contracts.Requests;

[ApiRoute("api/positions", ApiMethod.Create, ServiceName = "Positions")]
public record CreatePositionRequest(
    [property: Required(AllowEmptyStrings = false)]
    string Name) : ICrmRequest;
