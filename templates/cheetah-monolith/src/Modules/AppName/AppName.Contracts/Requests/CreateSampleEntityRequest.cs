using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Requests;

namespace AppName.Contracts.Requests;

public record CreateSampleEntityRequest(
    [property: Required(AllowEmptyStrings = false)]
    string Name,
    string? Description) : ICrmRequest;
