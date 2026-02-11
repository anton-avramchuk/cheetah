using Cheetah.Core.CQRS;

namespace Crm.Candidates.Application.Queries;

public record GetCandidateSourceByIdQuery(Guid Id) : IQuery<CandidateSourceModel?>;
