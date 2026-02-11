using Cheetah.Contracts.Responses;

namespace Crm.Candidates.Contracts.Response;

public record CandidateStageViewModel(Guid Id, string Name, int Order, string? Color, bool IsDefault) : ICrmResponse;
