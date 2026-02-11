using Cheetah.Contracts.Responses;

namespace Crm.Candidates.Contracts.Response;

public record CandidateViewModel(
    Guid Id,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    string? City,
    string? CurrentPosition,
    string? CurrentCompany,
    decimal? SalaryExpectation,
    string? About) : ICrmResponse;
