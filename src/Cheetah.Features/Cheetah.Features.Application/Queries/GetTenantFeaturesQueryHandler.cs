using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Features.DataAccess;
using Cheetah.Features.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Features.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetTenantFeaturesQuery, IReadOnlyList<TenantFeature>>))]
public class GetTenantFeaturesQueryHandler(
    FeaturesDbContext dbContext
) : IQueryHandler<GetTenantFeaturesQuery, IReadOnlyList<TenantFeature>>
{
    public async ValueTask<IReadOnlyList<TenantFeature>> HandleAsync(GetTenantFeaturesQuery query, CancellationToken cancellationToken = default)
    {
        return await dbContext.TenantFeatures
            .AsNoTracking()
            .Where(tf => tf.TenantId == query.TenantId)
            .OrderBy(tf => tf.FeatureId)
            .ToListAsync(cancellationToken);
    }
}
