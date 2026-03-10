using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Core;
using Crm.Candidates.Domain;
using Cheetah.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.Candidates.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetCandidateApplicationByIdQuery, CandidateApplicationModel?>))]
public class GetCandidateApplicationByIdQueryHandler(IRepository<CandidateApplication, Guid> repository, IObjectMapper mapper)
    : IQueryHandler<GetCandidateApplicationByIdQuery, CandidateApplicationModel?>
{
    public async ValueTask<CandidateApplicationModel?> HandleAsync(GetCandidateApplicationByIdQuery query, CancellationToken ct = default)
    {
        return await mapper
            .ProjectTo<CandidateApplicationModel>(repository.AsNoTrackingQueryable().Where(new EntityByIdSpecification<CandidateApplication>(query.Id)))
            .FirstOrDefaultAsync(ct);
    }
}
