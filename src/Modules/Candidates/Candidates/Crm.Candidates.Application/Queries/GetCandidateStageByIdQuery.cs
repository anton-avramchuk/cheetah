using Cheetah.Core.CQRS;

namespace Crm.Candidates.Application.Queries;

public record GetCandidateStageByIdQuery(Guid Id) : IQuery<CandidateStageModel?>;
