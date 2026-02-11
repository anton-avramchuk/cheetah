using Cheetah.Contracts.Responses;

namespace Crm.Candidates.Contracts.Response;

public record CandidateSourceViewModel(Guid Id, string Name, int Order, string? Color) : ICrmResponse;
