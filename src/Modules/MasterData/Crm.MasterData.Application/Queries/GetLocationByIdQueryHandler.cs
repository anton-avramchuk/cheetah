using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.MasterData.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.MasterData.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetLocationByIdQuery, LocationModel?>))]
public class GetLocationByIdQueryHandler(IRepository<Location, Guid> repository)
    : IQueryHandler<GetLocationByIdQuery, LocationModel?>
{
    public async ValueTask<LocationModel?> HandleAsync(GetLocationByIdQuery query, CancellationToken ct = default)
    {
        var entity = await repository.AsNoTrackingQueryable()
            .FirstOrDefaultAsync(e => e.Id == query.Id, ct);

        if (entity is null)
            return null;

        return new LocationModel(entity.Id, entity.Country, entity.City, entity.Timezone);
    }
}
