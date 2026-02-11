using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.Candidates.Contracts.Requests;

[ApiRoute("api/candidates/{id:guid}", ApiMethod.Update)]
public record UpdateCandidateRequest(
    [FromRoute] Guid Id,
    [property: Required(AllowEmptyStrings = false)]
    string FirstName,
    [property: Required(AllowEmptyStrings = false)]
    string LastName,
    string? Email,
    string? Phone,
    string? City,
    string? CurrentPosition,
    string? CurrentCompany,
    decimal? SalaryExpectation,
    string? About) : ICrmRequest;
