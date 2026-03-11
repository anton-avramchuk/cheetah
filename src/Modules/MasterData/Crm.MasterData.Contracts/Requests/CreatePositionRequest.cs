using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Requests;
using Crm.MasterData.Domain;

namespace Crm.MasterData.Contracts.Requests;

public record CreatePositionRequest(
    [property: Required(AllowEmptyStrings = false)]
    string Name,
    Grade Grade) : ICrmRequest;
