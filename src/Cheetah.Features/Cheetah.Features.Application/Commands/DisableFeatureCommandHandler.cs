using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Cheetah.Features.DataAccess;
using Cheetah.Features.Domain.Entities;
using Cheetah.Features.Events;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Features.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<DisableFeatureCommand>))]
public class DisableFeatureCommandHandler(
    FeaturesDbContext dbContext,
    IEventBus eventBus
) : ICommandHandler<DisableFeatureCommand>
{
    public async ValueTask HandleAsync(DisableFeatureCommand command, CancellationToken cancellationToken = default)
    {
        var tenantFeature = await dbContext.TenantFeatures
            .FirstOrDefaultAsync(tf => tf.TenantId == command.TenantId && tf.FeatureId == command.FeatureId, cancellationToken);

        if (tenantFeature == null)
        {
            // Create as disabled
            tenantFeature = TenantFeature.Create(command.TenantId, command.FeatureId, false);
            dbContext.TenantFeatures.Add(tenantFeature);
        }
        else
        {
            tenantFeature.Disable();
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        // Publish event
        await eventBus.PublishAsync(new FeatureDisabledEvent(
            command.TenantId,
            command.FeatureId,
            DateTime.UtcNow
        ), cancellationToken);
    }
}
