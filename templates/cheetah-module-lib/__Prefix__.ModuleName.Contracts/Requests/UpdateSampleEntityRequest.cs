using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Requests;

namespace __Prefix__.ModuleName.Contracts.Requests;

public record UpdateSampleEntityRequest(
    Guid Id,
    [property: Required(AllowEmptyStrings = false)]
    string Name,
    string? Description) : ICrmRequest;
