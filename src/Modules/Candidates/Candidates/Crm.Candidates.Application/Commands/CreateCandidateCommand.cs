using Cheetah.Core.CQRS;

namespace Crm.Candidates.Application.Commands;

public record CreateCandidateCommand(
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    string? City,
    string? CurrentPosition,
    string? CurrentCompany,
    decimal? SalaryExpectation,
    string? About) : ICommand<Guid>;
