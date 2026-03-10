using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Core;
using Crm.Candidates.Domain;
using Cheetah.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.Candidates.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetCandidateByIdQuery, CandidateModel?>))]
public class GetCandidateByIdQueryHandler(IRepository<Candidate, Guid> repository, IObjectMapper mapper)
    : IQueryHandler<GetCandidateByIdQuery, CandidateModel?>
{
    public async ValueTask<CandidateModel?> HandleAsync(GetCandidateByIdQuery query, CancellationToken ct = default)
    {
        return await mapper
            .ProjectTo<CandidateModel>(repository.AsNoTrackingQueryable().Where(new EntityByIdSpecification<Candidate>(query.Id)))
            .FirstOrDefaultAsync(ct);
    }
}
