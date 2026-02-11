using Cheetah.Core.CQRS;

namespace Crm.Candidates.Application.Queries;

public record GetCandidateByIdQuery(Guid Id) : IQuery<CandidateModel?>;