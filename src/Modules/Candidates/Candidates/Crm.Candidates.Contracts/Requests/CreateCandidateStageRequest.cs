using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.Candidates.Contracts.Requests;

[ApiRoute("api/candidate-stages", ApiMethod.Create, ServiceName = "CandidateStages")]
public record CreateCandidateStageRequest(
    [property: Required(AllowEmptyStrings = false)]
    string Name,
    int Order,
    string? Color,
    bool IsDefault) : ICrmRequest;
