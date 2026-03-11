using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.MasterData.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.MasterData.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetWorkFormatByIdQuery, WorkFormatModel?>))]
public class GetWorkFormatByIdQueryHandler(IRepository<WorkFormat, Guid> repository)
    : IQueryHandler<GetWorkFormatByIdQuery, WorkFormatModel?>
{
    public async ValueTask<WorkFormatModel?> HandleAsync(GetWorkFormatByIdQuery query, CancellationToken ct = default)
    {
        var entity = await repository.AsNoTrackingQueryable()
            .FirstOrDefaultAsync(e => e.Id == query.Id, ct);

        if (entity is null)
            return null;

        return new WorkFormatModel(entity.Id, entity.Name);
    }
}
