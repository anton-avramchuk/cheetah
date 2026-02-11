using Cheetah.Core.CQRS;

namespace Crm.Candidates.Application.Queries;

public record GetAllCandidateSourcesQuery : IQuery<IReadOnlyList<CandidateSourceModel>>;
