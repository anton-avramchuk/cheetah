using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.MasterData.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.MasterData.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetPositionByIdQuery, PositionModel?>))]
public class GetPositionByIdQueryHandler(IRepository<Position, Guid> repository)
    : IQueryHandler<GetPositionByIdQuery, PositionModel?>
{
    public async ValueTask<PositionModel?> HandleAsync(GetPositionByIdQuery query, CancellationToken ct = default)
    {
        var entity = await repository.AsNoTrackingQueryable()
            .FirstOrDefaultAsync(e => e.Id == query.Id, ct);

        if (entity is null)
            return null;

        return new PositionModel(entity.Id, entity.Name, entity.Grade);
    }
}
