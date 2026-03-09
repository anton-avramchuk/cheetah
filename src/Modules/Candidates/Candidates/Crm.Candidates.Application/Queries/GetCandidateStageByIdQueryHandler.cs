using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Core;
using Crm.Candidates.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.Candidates.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetCandidateStageByIdQuery, CandidateStageModel?>))]
public class GetCandidateStageByIdQueryHandler(IRepository<CandidateStage, Guid> repository, IObjectMapper mapper)
    : IQueryHandler<GetCandidateStageByIdQuery, CandidateStageModel?>
{
    public async ValueTask<CandidateStageModel?> HandleAsync(GetCandidateStageByIdQuery query, CancellationToken ct = default)
    {
        return await mapper
            .ProjectTo<CandidateStageModel>(repository.AsNoTrackingQueryable().Where(e => e.Id == query.Id))
            .FirstOrDefaultAsync(ct);
    }
}
