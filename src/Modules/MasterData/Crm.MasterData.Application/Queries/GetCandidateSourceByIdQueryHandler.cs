using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.MasterData.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.MasterData.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetCandidateSourceByIdQuery, CandidateSourceModel?>))]
public class GetCandidateSourceByIdQueryHandler(IRepository<CandidateSource, Guid> repository)
    : IQueryHandler<GetCandidateSourceByIdQuery, CandidateSourceModel?>
{
    public async ValueTask<CandidateSourceModel?> HandleAsync(GetCandidateSourceByIdQuery query, CancellationToken ct = default)
    {
        var entity = await repository.AsNoTrackingQueryable()
            .FirstOrDefaultAsync(e => e.Id == query.Id, ct);

        if (entity is null)
            return null;

        return new CandidateSourceModel(entity.Id, entity.Name);
    }
}
