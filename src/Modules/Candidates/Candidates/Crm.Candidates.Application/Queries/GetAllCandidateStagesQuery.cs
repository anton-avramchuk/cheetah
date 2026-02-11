using Cheetah.Core.CQRS;

namespace Crm.Candidates.Application.Queries;

public record GetAllCandidateStagesQuery : IQuery<IReadOnlyList<CandidateStageModel>>;
