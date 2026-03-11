using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.MasterData.Domain;

namespace Crm.MasterData.Contracts.Requests;

public record UpdatePositionRequest(
    [FromRoute] Guid Id,
    [property: Required(AllowEmptyStrings = false)]
    string Name,
    Grade Grade) : ICrmRequest;
