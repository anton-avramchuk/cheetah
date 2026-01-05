using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Features.DataAccess;
using Cheetah.Features.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Features.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<CheckFeatureQuery, bool>))]
public class CheckFeatureQueryHandler(
    FeaturesDbContext dbContext
) : IQueryHandler<CheckFeatureQuery, bool>
{
    public async ValueTask<bool> HandleAsync(CheckFeatureQuery query, CancellationToken cancellationToken = default)
    {
        // Get feature default setting
        var feature = await dbContext.Features
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == query.FeatureId, cancellationToken);

        if (feature == null)
            return false;

        // Check tenant-specific setting
        var tenantFeature = await dbContext.TenantFeatures
            .AsNoTracking()
            .FirstOrDefaultAsync(tf => tf.TenantId == query.TenantId && tf.FeatureId == query.FeatureId, cancellationToken);

        // If tenant has specific setting, use it; otherwise use default
        return tenantFeature?.IsEnabled ?? feature.IsEnabledByDefault;
    }
}
