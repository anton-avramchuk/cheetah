using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Cheetah.Features.DataAccess;
using Cheetah.Features.Domain.Entities;
using Cheetah.Features.Events;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Features.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<EnableFeatureCommand>))]
public class EnableFeatureCommandHandler(
    FeaturesDbContext dbContext,
    IEventBus eventBus
) : ICommandHandler<EnableFeatureCommand>
{
    public async ValueTask HandleAsync(EnableFeatureCommand command, CancellationToken cancellationToken = default)
    {
        // Check if feature exists
        var featureExists = await dbContext.Features
            .AnyAsync(f => f.Id == command.FeatureId, cancellationToken);

        if (!featureExists)
            throw new InvalidOperationException($"Feature '{command.FeatureId}' not found");

        // Get or create tenant feature
        var tenantFeature = await dbContext.TenantFeatures
            .FirstOrDefaultAsync(tf => tf.TenantId == command.TenantId && tf.FeatureId == command.FeatureId, cancellationToken);

        if (tenantFeature == null)
        {
            tenantFeature = TenantFeature.Create(command.TenantId, command.FeatureId, true);
            dbContext.TenantFeatures.Add(tenantFeature);
        }
        else
        {
            tenantFeature.Enable();
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        // Publish event
        await eventBus.PublishAsync(new FeatureEnabledEvent(
            command.TenantId,
            command.FeatureId,
            DateTime.UtcNow
        ), cancellationToken);
    }
}
