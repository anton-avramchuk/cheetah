using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.MasterData.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.MasterData.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetIndustryByIdQuery, IndustryModel?>))]
public class GetIndustryByIdQueryHandler(IRepository<Industry, Guid> repository)
    : IQueryHandler<GetIndustryByIdQuery, IndustryModel?>
{
    public async ValueTask<IndustryModel?> HandleAsync(GetIndustryByIdQuery query, CancellationToken ct = default)
    {
        var entity = await repository.AsNoTrackingQueryable()
            .FirstOrDefaultAsync(e => e.Id == query.Id, ct);

        if (entity is null)
            return null;

        return new IndustryModel(entity.Id, entity.Name);
    }
}
