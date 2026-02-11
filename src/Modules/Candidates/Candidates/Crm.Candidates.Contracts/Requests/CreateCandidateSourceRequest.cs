using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.Candidates.Contracts.Requests;

[ApiRoute("api/candidate-sources", ApiMethod.Create, ServiceName = "CandidateSources")]
public record CreateCandidateSourceRequest(
    [property: Required(AllowEmptyStrings = false)]
    string Name,
    int Order,
    string? Color) : ICrmRequest;
