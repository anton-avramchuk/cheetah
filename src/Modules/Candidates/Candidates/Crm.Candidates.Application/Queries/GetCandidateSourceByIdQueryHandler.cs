using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Core;
using Crm.Candidates.Domain;
using Cheetah.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.Candidates.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetCandidateSourceByIdQuery, CandidateSourceModel?>))]
public class GetCandidateSourceByIdQueryHandler(IRepository<CandidateSource, Guid> repository, IObjectMapper mapper)
    : IQueryHandler<GetCandidateSourceByIdQuery, CandidateSourceModel?>
{
    public async ValueTask<CandidateSourceModel?> HandleAsync(GetCandidateSourceByIdQuery query, CancellationToken ct = default)
    {
        return await mapper
            .ProjectTo<CandidateSourceModel>(repository.AsNoTrackingQueryable().Where(new EntityByIdSpecification<CandidateSource>(query.Id)))
            .FirstOrDefaultAsync(ct);
    }
}
