using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Core;
using Crm.Recruitment.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.Recruitment.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetWorkFormatByIdQuery, WorkFormatModel?>))]
public class GetWorkFormatByIdQueryHandler(IRepository<WorkFormat, Guid> repository, IObjectMapper mapper)
    : IQueryHandler<GetWorkFormatByIdQuery, WorkFormatModel?>
{
    public async ValueTask<WorkFormatModel?> HandleAsync(GetWorkFormatByIdQuery query, CancellationToken ct = default)
    {
        return await mapper
            .ProjectTo<WorkFormatModel>(repository.AsNoTrackingQueryable().Where(e => e.Id == query.Id))
            .FirstOrDefaultAsync(ct);
    }
}
