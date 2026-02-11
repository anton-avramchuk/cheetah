namespace Crm.Candidates.Application;

public record CandidateStageModel(Guid Id, string Name, int Order, string? Color, bool IsDefault);
