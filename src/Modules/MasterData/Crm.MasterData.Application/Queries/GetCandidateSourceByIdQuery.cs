using Cheetah.Core.CQRS;

namespace Crm.MasterData.Application.Queries;

public record GetCandidateSourceByIdQuery(Guid Id) : IQuery<CandidateSourceModel?>;
