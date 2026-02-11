using Cheetah.Core.CQRS;

namespace Crm.Candidates.Application.Queries;

public record GetCandidateApplicationByIdQuery(Guid Id) : IQuery<CandidateApplicationModel?>;
