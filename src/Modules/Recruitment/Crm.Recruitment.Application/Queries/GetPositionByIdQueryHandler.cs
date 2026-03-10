using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Core;
using Crm.Recruitment.Domain;
using Crm.Recruitment.Domain.Specifications;
using Microsoft.EntityFrameworkCore;

namespace Crm.Recruitment.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetPositionByIdQuery, PositionModel?>))]
public class GetPositionByIdQueryHandler(IRepository<Position, Guid> repository, IObjectMapper mapper)
    : IQueryHandler<GetPositionByIdQuery, PositionModel?>
{
    public async ValueTask<PositionModel?> HandleAsync(GetPositionByIdQuery query, CancellationToken ct = default)
    {
        return await mapper
            .ProjectTo<PositionModel>(repository.AsNoTrackingQueryable().Where(new EntityByIdSpecification<Position>(query.Id)))
            .FirstOrDefaultAsync(ct);
    }
}
