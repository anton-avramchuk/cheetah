using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Requests;

namespace Crm.MasterData.Contracts.Requests;

public record CreateStackItemRequest(
    [property: Required(AllowEmptyStrings = false)]
    string Name,
    string? Description) : ICrmRequest;