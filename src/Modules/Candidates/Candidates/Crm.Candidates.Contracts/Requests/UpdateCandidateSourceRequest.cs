using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.Candidates.Contracts.Requests;

[ApiRoute("api/candidate-sources/{id:guid}", ApiMethod.Update, ServiceName = "CandidateSources")]
public record UpdateCandidateSourceRequest(
    [FromRoute] Guid Id,
    [property: Required(AllowEmptyStrings = false)]
    string Name,
    int Order,
    string? Color) : ICrmRequest;
