using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.MasterData.Domain;

namespace Crm.MasterData.Contracts.Requests;

[ApiRoute("api/positions", ApiMethod.Create, ServiceName = "Position")]
public record CreatePositionRequest(
    [property: Required(AllowEmptyStrings = false)]
    string Name,
    Grade Grade) : ICrmRequest;
