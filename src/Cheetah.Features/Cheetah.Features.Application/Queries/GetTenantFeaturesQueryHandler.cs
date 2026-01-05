using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Features.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Features.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetTenantFeaturesQuery, IReadOnlyList<TenantFeature>>))]
public class GetTenantFeaturesQueryHandler(
    IRepository<TenantFeature, Guid> repository
) : IQueryHandler<GetTenantFeaturesQuery, IReadOnlyList<TenantFeature>>
{
    public async ValueTask<IReadOnlyList<TenantFeature>> HandleAsync(GetTenantFeaturesQuery query, CancellationToken cancellationToken = default)
    {
        return await repository.GetQuery()
            .AsNoTracking()
            .Where(tf => tf.TenantId == query.TenantId)
            .OrderBy(tf => tf.FeatureId)
            .ToListAsync(cancellationToken);
    }
}
