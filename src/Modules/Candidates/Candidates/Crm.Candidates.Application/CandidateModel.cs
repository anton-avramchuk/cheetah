namespace Crm.Candidates.Application;

public record CandidateModel(
    Guid Id,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    string? City,
    string? CurrentPosition,
    string? CurrentCompany,
    decimal? SalaryExpectation,
    string? About);
