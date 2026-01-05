using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Cheetah.Features.Domain.Entities;
using Cheetah.Features.Events;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Features.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<DisableFeatureCommand>))]
public class DisableFeatureCommandHandler(
    IRepository<TenantFeature, Guid> repository,
    IEventBus eventBus
) : ICommandHandler<DisableFeatureCommand>
{
    public async ValueTask HandleAsync(DisableFeatureCommand command, CancellationToken cancellationToken = default)
    {
        var tenantFeature = await repository.GetQuery()
            .FirstOrDefaultAsync(tf => tf.TenantId == command.TenantId && tf.FeatureId == command.FeatureId, cancellationToken);

        if (tenantFeature == null)
        {
            // Create as disabled
            tenantFeature = TenantFeature.Create(command.TenantId, command.FeatureId, false);
            await repository.InsertAsync(tenantFeature, cancellationToken);
        }
        else
        {
            tenantFeature.Disable();
            await repository.UpdateAsync(tenantFeature, cancellationToken);
        }

        // Publish event
        await eventBus.PublishAsync(new FeatureDisabledEvent(
            command.TenantId,
            command.FeatureId,
            DateTime.UtcNow
        ), cancellationToken);
    }
}
