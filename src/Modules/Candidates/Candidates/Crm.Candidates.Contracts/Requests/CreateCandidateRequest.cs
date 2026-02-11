using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Requests;

namespace Crm.Candidates.Contracts.Requests;

public record CreateCandidateRequest(
    [property: Required(AllowEmptyStrings = false)]
    string Name,
    string? Description) : ICrmRequest;