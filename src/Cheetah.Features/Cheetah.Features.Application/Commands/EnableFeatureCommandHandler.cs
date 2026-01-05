using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Cheetah.Features.Domain.Entities;
using Cheetah.Features.Events;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Features.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<EnableFeatureCommand>))]
public class EnableFeatureCommandHandler(
    IRepository<TenantFeature, Guid> repository,
    IRepository<Feature, string> featureRepository,
    IEventBus eventBus
) : ICommandHandler<EnableFeatureCommand>
{
    public async ValueTask HandleAsync(EnableFeatureCommand command, CancellationToken cancellationToken = default)
    {
        // Check if feature exists
        var featureExists = await featureRepository.GetQuery()
            .AnyAsync(f => f.Id == command.FeatureId, cancellationToken);

        if (!featureExists)
            throw new InvalidOperationException($"Feature '{command.FeatureId}' not found");

        // Get or create tenant feature
        var tenantFeature = await repository.GetQuery()
            .FirstOrDefaultAsync(tf => tf.TenantId == command.TenantId && tf.FeatureId == command.FeatureId, cancellationToken);

        if (tenantFeature == null)
        {
            tenantFeature = TenantFeature.Create(command.TenantId, command.FeatureId, true);
            await repository.InsertAsync(tenantFeature, cancellationToken);
        }
        else
        {
            tenantFeature.Enable();
            await repository.UpdateAsync(tenantFeature, cancellationToken);
        }

        // Publish event
        await eventBus.PublishAsync(new FeatureEnabledEvent(
            command.TenantId,
            command.FeatureId,
            DateTime.UtcNow
        ), cancellationToken);
    }
}
