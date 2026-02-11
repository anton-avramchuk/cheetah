using Cheetah.Contracts.Responses;

namespace Crm.Candidates.Contracts.Response;

public record CandidateViewModel(Guid Id, string Name, string? Description) : ICrmResponse;